// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
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
    /// container/{channelId]/{conversationId}/{Timestamp.ticks}-{activity.id}.json
    /// </remarks>
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
        /// Creates an instance of AzureBlobTranscriptStore
        /// </summary>
        /// <param name="dataConnectionstring">Connection string to connect to Azure Blob Storage</param>
        /// <param name="containerName">Name of the continer where transcript blobs will be stored</param>
        public AzureBlobTranscriptStore(string dataConnectionstring, string containerName)
            : this(CloudStorageAccount.Parse(dataConnectionstring), containerName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobTranscriptStore"/> class.
        /// </summary>
        /// <param name="storageAccount">Azure Storage Account to store transcripts</param>
        /// <param name="containerName">Name of the continer where transcript blobs will be stored</param>
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
                containerName = containerName.ToLower();
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
        /// <returns></returns>
        public async Task LogActivityAsync(IActivity activity)
        {
            BotAssert.ActivityNotNull(activity);

            var blobName = GetBlobName(activity);
            var blobReference = this.Container.Value.GetBlockBlobReference(blobName);
            blobReference.Properties.ContentType = "application/json";
            blobReference.Metadata["FromId"] = activity.From.Id;
            blobReference.Metadata["RecipientId"] = activity.Recipient.Id;
            blobReference.Metadata["Timestamp"] = activity.Timestamp.Value.ToString("O");
            using (var blobStream = await blobReference.OpenWriteAsync().ConfigureAwait(false))
            {
                using (var jsonWriter = new JsonTextWriter(new StreamWriter(blobStream)))
                {
                    _jsonSerializer.Serialize(jsonWriter, activity);
                }
            }

            await blobReference.SetMetadataAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Get activities for a conversation (Aka the transcript)
        /// </summary>
        /// <param name="channelId">Channel Id.</param>
        /// <param name="conversationId">Conversation Id.</param>
        /// <param name="continuationToken">Continuatuation token to page through results.</param>
        /// <param name="startDate">Earliest time to include.</param>
        /// <returns>PagedResult of activities</returns>
        public async Task<PagedResult<IActivity>> GetTranscriptActivitiesAsync(string channelId, string conversationId, string continuationToken = null, DateTimeOffset startDate = default(DateTimeOffset))
        {
            if (string.IsNullOrEmpty(channelId))
            {
                throw new ArgumentNullException($"missing {nameof(channelId)}");
            }

            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException($"missing {nameof(conversationId)}");
            }

            var pagedResult = new PagedResult<IActivity>();

            var dirName = GetDirName(channelId, conversationId);
            var dir = this.Container.Value.GetDirectoryReference(dirName);
            int pageSize = 20;
            BlobContinuationToken token = null;
            List<CloudBlockBlob> blobs = new List<CloudBlockBlob>();
            do
            {
                var segment = await dir.ListBlobsSegmentedAsync(false, BlobListingDetails.Metadata, null, token, null, null).ConfigureAwait(false);

                foreach (var blob in segment.Results.Cast<CloudBlockBlob>())
                {
                    if (DateTime.Parse(blob.Metadata["Timestamp"]).ToUniversalTime() >= startDate)
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

                if (segment.ContinuationToken != null)
                {
                    token = segment.ContinuationToken;
                }
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
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<PagedResult<Transcript>> ListTranscriptsAsync(string channelId, string continuationToken = null)
        {
            if (string.IsNullOrEmpty(channelId))
            {
                throw new ArgumentNullException($"missing {nameof(channelId)}");
            }

            var dirName = GetDirName(channelId);
            var dir = this.Container.Value.GetDirectoryReference(dirName);
            int pageSize = 20;
            BlobContinuationToken token = null;
            List<Transcript> conversations = new List<Transcript>();
            do
            {
                var segment = await dir.ListBlobsSegmentedAsync(false, BlobListingDetails.Metadata, null, token, null, null).ConfigureAwait(false);

                foreach (var blob in segment.Results.Where(c => c is CloudBlobDirectory).Cast<CloudBlobDirectory>())
                {
                    var conversation = new Transcript() { Id = blob.Prefix.Split('/').Where(s => s.Length > 0).Last(), ChannelId = channelId };
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

            var pagedResult = new PagedResult<Transcript>();
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
        /// <returns></returns>
        public async Task DeleteTranscriptAsync(string channelId, string conversationId)
        {
            if (string.IsNullOrEmpty(channelId))
            {
                throw new ArgumentNullException($"{nameof(channelId)} should not be null");
            }

            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException($"{nameof(conversationId)} should not be null");
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

                if (segment.ContinuationToken != null)
                {
                    token = segment.ContinuationToken;
                }
            }
            while (token != null);
        }

        private static string GetBlobName(IActivity activity)
        {
            var blobName = $"{SanitizeKey(activity.ChannelId)}/{SanitizeKey(activity.Conversation.Id)}/{activity.Timestamp.Value.Ticks.ToString("x")}-{SanitizeKey(activity.Id)}.json";
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
    }
}
