// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Azure
{
    /// <summary>
    /// The blob transcript store stores transcripts in an Azure Blob container.
    /// </summary>
    /// <remarks>
    /// Each activity is stored as json blob in structure of
    /// container/{channelId]/{conversationId}/{Timestamp.ticks}-{activity.id}.json.
    /// </remarks>
    [Obsolete("This class is deprecated. Please use BlobsTranscriptStore from Microsoft.Bot.Builder.Azure.Blobs instead.")]
    public class AzureBlobTranscriptStore : ITranscriptStore
    {
        private static readonly JsonSerializer _jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented,
        });

        private static HashSet<string> _checkedContainers = new HashSet<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobTranscriptStore"/> class.
        /// Creates an instance of AzureBlobTranscriptStore.
        /// </summary>
        /// <param name="dataConnectionstring">Connection string to connect to Azure Blob Storage.</param>
        /// <param name="containerName">Name of the container where transcript blobs will be stored.</param>
        public AzureBlobTranscriptStore(string dataConnectionstring, string containerName)
            : this(CloudStorageAccount.Parse(dataConnectionstring), containerName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobTranscriptStore"/> class.
        /// </summary>
        /// <param name="storageAccount">Azure Storage Account to store transcripts.</param>
        /// <param name="containerName">Name of the container where transcript blobs will be stored.</param>
        public AzureBlobTranscriptStore(CloudStorageAccount storageAccount, string containerName)
        {
            if (storageAccount == null)
            {
                throw new ArgumentNullException(nameof(storageAccount));
            }

            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentNullException(nameof(containerName));
            }

            this.Container = new Lazy<CloudBlobContainer>(
                () =>
            {
                containerName = containerName.ToLowerInvariant();
                var blobClient = storageAccount.CreateCloudBlobClient();
                NameValidator.ValidateContainerName(containerName);
                var container = blobClient.GetContainerReference(containerName);
                if (!_checkedContainers.Contains(containerName))
                {
                    _checkedContainers.Add(containerName);
                    container.CreateIfNotExistsAsync().Wait();
                }

                return container;
            }, isThreadSafe: true);
        }

        private Lazy<CloudBlobContainer> Container { get; set; }

        /// <summary>
        /// Log an activity to the transcript.
        /// </summary>
        /// <param name="activity">Activity being logged.</param>
        /// <returns>A <see cref="Task"/>A task that represents the work queued to execute.</returns>
        public async Task LogActivityAsync(IActivity activity)
        {
            BotAssert.ActivityNotNull(activity);

            switch (activity.Type)
            {
                case ActivityTypes.MessageUpdate:
                    {
                        var updatedActivity = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(activity));
                        updatedActivity.Type = ActivityTypes.Message; // fixup original type (should be Message)

                        var blob = await FindActivityBlobAsync(activity).ConfigureAwait(false);
                        if (blob != null)
                        {
                            var originalActivity = JsonConvert.DeserializeObject<Activity>(await blob.DownloadTextAsync().ConfigureAwait(false));

                            updatedActivity.LocalTimestamp = originalActivity.LocalTimestamp;
                            updatedActivity.Timestamp = originalActivity.Timestamp;
                            await LogActivityAsync(updatedActivity, blob).ConfigureAwait(false);
                        }
                        else
                        {
                            // The activity was not found, so just add a record of this update.
                            await InnerLogActivityAsync(updatedActivity).ConfigureAwait(false);
                        }

                        return;
                    }

                case ActivityTypes.MessageDelete:
                    {
                        var blob = await FindActivityBlobAsync(activity).ConfigureAwait(false);
                        if (blob != null)
                        {
                            var originalActivity = JsonConvert.DeserializeObject<Activity>(await blob.DownloadTextAsync().ConfigureAwait(false));

                            // tombstone the original message
                            var tombstonedActivity = new Activity()
                            {
                                Type = ActivityTypes.MessageDelete,
                                Id = originalActivity.Id,
                                From = new ChannelAccount(id: "deleted", role: originalActivity.From.Role),
                                Recipient = new ChannelAccount(id: "deleted", role: originalActivity.Recipient.Role),
                                Locale = originalActivity.Locale,
                                LocalTimestamp = originalActivity.Timestamp,
                                Timestamp = originalActivity.Timestamp,
                                ChannelId = originalActivity.ChannelId,
                                Conversation = originalActivity.Conversation,
                                ServiceUrl = originalActivity.ServiceUrl,
                                ReplyToId = originalActivity.ReplyToId,
                            };

                            await LogActivityAsync(tombstonedActivity, blob).ConfigureAwait(false);
                        }

                        return;
                    }

                default:
                    await InnerLogActivityAsync(activity).ConfigureAwait(false);
                    return;
            }
        }

        /// <summary>
        /// Get activities for a conversation (Aka the transcript).
        /// </summary>
        /// <param name="channelId">Channel Id.</param>
        /// <param name="conversationId">Conversation Id.</param>
        /// <param name="continuationToken">Continuatuation token to page through results.</param>
        /// <param name="startDate">Earliest time to include.</param>
        /// <returns>PagedResult of activities.</returns>
        public async Task<PagedResult<IActivity>> GetTranscriptActivitiesAsync(string channelId, string conversationId, string continuationToken = null, DateTimeOffset startDate = default(DateTimeOffset))
        {
            if (string.IsNullOrEmpty(channelId))
            {
                throw new ArgumentNullException(nameof(channelId));
            }

            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            var pagedResult = new PagedResult<IActivity>();

            var dirName = GetDirName(channelId, conversationId);
            var dir = this.Container.Value.GetDirectoryReference(dirName);
            var pageSize = 20;
            BlobContinuationToken token = null;
            List<CloudBlockBlob> blobs = new List<CloudBlockBlob>();
            do
            {
                var segment = await dir.ListBlobsSegmentedAsync(false, BlobListingDetails.Metadata, null, token, null, null).ConfigureAwait(false);

                foreach (var blob in segment.Results.Cast<CloudBlockBlob>())
                {
                    if (DateTime.Parse(blob.Metadata["Timestamp"], CultureInfo.InvariantCulture).ToUniversalTime() >= startDate)
                    {
                        if (continuationToken != null)
                        {
                            if (blob.Name == continuationToken)
                            {
                                // we found continuation token
                                continuationToken = null;
                            }
                        }
                        else
                        {
                            blobs.Add(blob);
                            if (blobs.Count == pageSize)
                            {
                                break;
                            }
                        }
                    }
                }

                token = segment.ContinuationToken;
            }
            while (token != null && blobs.Count < pageSize);

            pagedResult.Items = blobs
                .Select(async bl =>
                {
                    var json = await bl.DownloadTextAsync().ConfigureAwait(false);
                    return JsonConvert.DeserializeObject<Activity>(json);
                })
                .Select(t => t.Result)
                .ToArray();

            if (pagedResult.Items.Length == pageSize)
            {
                pagedResult.ContinuationToken = blobs.Last().Name;
            }

            return pagedResult;
        }

        /// <summary>
        /// List conversations in the channelId.
        /// </summary>
        /// <param name="channelId">Channel Id.</param>
        /// <param name="continuationToken">Continuatuation token to page through results.</param>
        /// <returns>A <see cref="Task"/> A task that represents the work queued to execute.</returns>
        public async Task<PagedResult<TranscriptInfo>> ListTranscriptsAsync(string channelId, string continuationToken = null)
        {
            if (string.IsNullOrEmpty(channelId))
            {
                throw new ArgumentNullException(nameof(channelId));
            }

            var dirName = GetDirName(channelId);
            var dir = this.Container.Value.GetDirectoryReference(dirName);
            var pageSize = 20;
            BlobContinuationToken token = null;
            List<TranscriptInfo> conversations = new List<TranscriptInfo>();
            do
            {
                var segment = await dir.ListBlobsSegmentedAsync(false, BlobListingDetails.Metadata, null, token, null, null).ConfigureAwait(false);

                foreach (var blob in segment.Results.Where(c => c is CloudBlobDirectory).Cast<CloudBlobDirectory>())
                {
                    // Unescape the Id we escaped when we saved it
                    var conversation = new TranscriptInfo() { Id = Uri.UnescapeDataString(blob.Prefix.Split('/').Where(s => s.Length > 0).Last()), ChannelId = channelId };
                    if (continuationToken != null)
                    {
                        if (conversation.Id == continuationToken)
                        {
                            // we found continuation token
                            continuationToken = null;
                        }

                        // skip record
                    }
                    else
                    {
                        conversations.Add(conversation);
                        if (conversations.Count == pageSize)
                        {
                            break;
                        }
                    }
                }

                if (segment.ContinuationToken != null)
                {
                    token = segment.ContinuationToken;
                }
            }
            while (token != null && conversations.Count < pageSize);

            var pagedResult = new PagedResult<TranscriptInfo>();
            pagedResult.Items = conversations.ToArray();

            if (pagedResult.Items.Length == 20)
            {
                pagedResult.ContinuationToken = pagedResult.Items.Last().Id;
            }

            return pagedResult;
        }

        /// <summary>
        /// Delete a specific conversation and all of it's activities.
        /// </summary>
        /// <param name="channelId">Channel Id where conversation took place.</param>
        /// <param name="conversationId">Id of the conversation to delete.</param>
        /// <returns>A <see cref="Task"/>A task that represents the work queued to execute.</returns>
        public async Task DeleteTranscriptAsync(string channelId, string conversationId)
        {
            if (string.IsNullOrEmpty(channelId))
            {
                throw new ArgumentNullException(nameof(channelId));
            }

            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            var dirName = GetDirName(channelId, conversationId);
            var dir = this.Container.Value.GetDirectoryReference(dirName);
            BlobContinuationToken token = null;
            List<CloudBlockBlob> blobs = new List<CloudBlockBlob>();
            do
            {
                var segment = await dir.ListBlobsSegmentedAsync(false, BlobListingDetails.None, null, token, null, null).ConfigureAwait(false);
                foreach (var blob in segment.Results.Cast<CloudBlockBlob>())
                {
                    await blob.DeleteIfExistsAsync().ConfigureAwait(false);
                }

                token = segment.ContinuationToken;
            }
            while (token != null);
        }

        private static async Task LogActivityAsync(IActivity activity, CloudBlockBlob blobReference)
        {
            blobReference.Properties.ContentType = "application/json";
            blobReference.Metadata["Id"] = activity.Id;
            blobReference.Metadata["FromId"] = activity.From.Id;
            blobReference.Metadata["RecipientId"] = activity.Recipient.Id;
            blobReference.Metadata["Timestamp"] = activity.Timestamp.Value.ToString("O", CultureInfo.InvariantCulture);
            using (var blobStream = await blobReference.OpenWriteAsync().ConfigureAwait(false))
            {
                using (var jsonWriter = new JsonTextWriter(new StreamWriter(blobStream)))
                {
                    _jsonSerializer.Serialize(jsonWriter, activity);
                }
            }

            await blobReference.SetMetadataAsync().ConfigureAwait(false);
        }

        private static string GetBlobName(IActivity activity)
        {
            var blobName = $"{SanitizeKey(activity.ChannelId)}/{SanitizeKey(activity.Conversation.Id)}/{activity.Timestamp.Value.Ticks.ToString("x", CultureInfo.InvariantCulture)}-{SanitizeKey(activity.Id)}.json";
            NameValidator.ValidateBlobName(blobName);
            return blobName;
        }

        private static string GetDirName(string channelId, string conversationId = null)
        {
            string dirName = string.Empty;
            if (conversationId != null)
            {
                var convId = SanitizeKey(conversationId);
                NameValidator.ValidateDirectoryName(channelId);
                NameValidator.ValidateDirectoryName(convId);
                dirName = $"{channelId}/{convId}";
            }
            else
            {
                NameValidator.ValidateDirectoryName(channelId);
                dirName = $"{channelId}";
            }

            return dirName;
        }

        private static string SanitizeKey(string key)
        {
            // Blob Name rules: case-sensitive any url char
            return Uri.EscapeDataString(key);
        }

        private Task InnerLogActivityAsync(IActivity activity)
        {
            var blobName = GetBlobName(activity);
            var blobReference = this.Container.Value.GetBlockBlobReference(blobName);
            return LogActivityAsync(activity, blobReference);
        }

        private async Task<CloudBlockBlob> FindActivityBlobAsync(IActivity activity)
        {
            var dirName = GetDirName(activity.ChannelId, activity.Conversation.Id);
            var dir = this.Container.Value.GetDirectoryReference(dirName);
            BlobContinuationToken token = null;
            do
            {
                var segment = await dir.ListBlobsSegmentedAsync(false, BlobListingDetails.Metadata, 50, token, null, null).ConfigureAwait(false);
                foreach (var blob in segment.Results.Cast<CloudBlockBlob>())
                {
                    blob.Metadata.TryGetValue("Id", out string id);
                    if (!string.IsNullOrEmpty(id))
                    {
                        if (id == activity.Id)
                        {
                            return blob;
                        }
                    }
                    else
                    {
                        // we have to read full activity as it's an old blob
                        var entry = JsonConvert.DeserializeObject<Activity>(await blob.DownloadTextAsync().ConfigureAwait(false));
                        blob.Metadata["Id"] = entry.Id;

                        // update metadata with Id so we don't have to download again.  This effectively "patches" old metadata records
                        await blob.SetMetadataAsync().ConfigureAwait(false);
                        if (entry.Id == activity.Id)
                        {
                            return blob;
                        }
                    }
                }

                token = segment.ContinuationToken;
            }
            while (token != null);

            return null;
        }
    }
}
