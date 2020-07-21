// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Azure.Storage
{
    /// <summary>
    /// The blobs transcript logger stores transcripts in an Azure Blob container.
    /// </summary>
    /// <remarks>
    /// Each activity is stored as json blob in structure of
    /// container/{channelId]/{conversationId}/{Timestamp.ticks}-{activity.id}.json.
    /// </remarks>
    public class BlobsTranscriptLogger : ITranscriptLogger
    {
        private static readonly JsonSerializer JsonSerializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            // we use All so that we get typed roundtrip out of storage, but we don't use validation because we don't know what types are valid
            TypeNameHandling = TypeNameHandling.All,
        });

        // If a JsonSerializer is not provided during construction, this will be the default static JsonSerializer.
        private readonly JsonSerializer _jsonSerializer;
        private readonly BlobContainerClient _containerClient;
        private int _checkforContainerExistance;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobsTranscriptLogger"/> class.
        /// </summary>
        /// <param name="dataConnectionstring">Azure Storage connection string.</param>
        /// <param name="containerName">Name of the Blob container where entities will be stored.</param>
        /// <param name="jsonSerializer">If passing in a custom JsonSerializer, we recommend the following settings:
        /// <para>jsonSerializer.TypeNameHandling = TypeNameHandling.All.</para>
        /// <para>jsonSerializer.NullValueHandling = NullValueHandling.Include.</para>
        /// <para>jsonSerializer.ContractResolver = new DefaultContractResolver().</para>
        /// </param>
        public BlobsTranscriptLogger(string dataConnectionstring, string containerName, JsonSerializer jsonSerializer = null)
        {
            if (string.IsNullOrEmpty(dataConnectionstring))
            {
                throw new ArgumentNullException(nameof(dataConnectionstring));
            }

            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentNullException(nameof(containerName));
            }

            _jsonSerializer = jsonSerializer ?? JsonSerializer;

            // Triggers a check for the existance of the container
            _checkforContainerExistance = 1;

            _containerClient = new BlobContainerClient(dataConnectionstring, containerName);
        }

        /// <summary>
        /// Log an activity to the transcript.
        /// </summary>
        /// <param name="activity">Activity being logged.</param>
        /// <returns>A <see cref="Task"/>A task that represents the work queued to execute.</returns>
        public async Task LogActivityAsync(IActivity activity)
        {
            BotAssert.ActivityNotNull(activity);

            // this should only happen once - assuming this is a singleton
            if (Interlocked.CompareExchange(ref _checkforContainerExistance, 0, 1) == 1)
            {
                await _containerClient.CreateIfNotExistsAsync().ConfigureAwait(false);
            }

            switch (activity.Type)
            {
                case ActivityTypes.MessageUpdate:
                    {
                        var activityAndBlob = await InnerReadBlobAsync(activity).ConfigureAwait(false);
                        if (activityAndBlob.Item1 != null)
                        {
                            var updatedActivity = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(activity));
                            updatedActivity.Type = ActivityTypes.Message; // fixup original type (should be Message)
                            updatedActivity.LocalTimestamp = activityAndBlob.Item1.LocalTimestamp;
                            updatedActivity.Timestamp = activityAndBlob.Item1.Timestamp;
                            await LogActivityAsync(updatedActivity, activityAndBlob.Item2, true).ConfigureAwait(false);
                        }

                        return;
                    }

                case ActivityTypes.MessageDelete:
                    {
                        var activityAndBlob = await InnerReadBlobAsync(activity).ConfigureAwait(false);
                        if (activityAndBlob.Item1 != null)
                        {
                            // tombstone the original message
                            var tombstonedActivity = new Activity()
                            {
                                Type = ActivityTypes.MessageDelete,
                                Id = activityAndBlob.Item1.Id,
                                From = new ChannelAccount(id: "deleted", role: activityAndBlob.Item1.From.Role),
                                Recipient = new ChannelAccount(id: "deleted", role: activityAndBlob.Item1.Recipient.Role),
                                Locale = activityAndBlob.Item1.Locale,
                                LocalTimestamp = activityAndBlob.Item1.Timestamp,
                                Timestamp = activityAndBlob.Item1.Timestamp,
                                ChannelId = activityAndBlob.Item1.ChannelId,
                                Conversation = activityAndBlob.Item1.Conversation,
                                ServiceUrl = activityAndBlob.Item1.ServiceUrl,
                                ReplyToId = activityAndBlob.Item1.ReplyToId,
                            };

                            await LogActivityAsync(tombstonedActivity, activityAndBlob.Item2, true).ConfigureAwait(false);
                        }

                        return;
                    }

                default:
                    var blobName = GetBlobName(activity);
                    var blobReference = _containerClient.GetBlobClient(blobName);
                    await LogActivityAsync(activity, blobReference).ConfigureAwait(false);
                    return;
            }
        }

        /// <summary>
        /// Get an Activity from Blob Storage based on the provided IActivity as the key.
        /// </summary>
        /// <param name="originalActivity">The activity to use as the key for searching Blob Storage.</param>
        /// <returns>An activity object retrieved from Blob Storage, or null if not found.</returns>
        protected async Task<Activity> GetActivityAsync(IActivity originalActivity)
        {
            var activityAndBlobClient = await InnerReadBlobAsync(originalActivity).ConfigureAwait(false);
            return activityAndBlobClient.Item1;
        }

        private async Task<(Activity, BlobClient)> InnerReadBlobAsync(IActivity activity)
        {
            var i = 0;
            while (true)
            {
                try
                {
                    string token = null;
                    do
                    {
                        var resultSegment = _containerClient.GetBlobs(BlobTraits.Metadata, prefix: $"{SanitizeKey(activity.ChannelId)}/{SanitizeKey(activity.Conversation.Id)}/{SanitizeKey(activity.Id)}-")
                                                            .AsPages(token, 1);

                        foreach (var blobPage in resultSegment)
                        {
                            foreach (BlobItem blobItem in blobPage.Values)
                            {
                                if (blobItem.Metadata.TryGetValue("Id", out string id) && id == activity.Id)
                                {
                                    var blobClient = _containerClient.GetBlobClient(blobItem.Name);
                                    using (BlobDownloadInfo download = await blobClient.DownloadAsync().ConfigureAwait(false))
                                    {
                                        using (var jsonReader = new JsonTextReader(new StreamReader(download.Content)))
                                        {
                                            var resultActivity = _jsonSerializer.Deserialize(jsonReader, typeof(Activity)) as Activity;
                                            return (resultActivity, blobClient);
                                        }
                                    }
                                }
                            }

                            // Get the continuation token and loop until it is empty.
                            token = blobPage.ContinuationToken;
                        }
                    } 
                    while (!string.IsNullOrEmpty(token));
                }
                catch (RequestFailedException ex)
                    when ((HttpStatusCode)ex.Status == HttpStatusCode.PreconditionFailed)
                {
                    // additional retry logic, even though this is a read operation blob storage can return 412 if there is contention
                    if (i++ < 3)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
                        continue;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        private async Task LogActivityAsync(IActivity activity, BlobClient blobReference, bool overwrite = false)
        {
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var jsonWriter = new JsonTextWriter(streamWriter))
            {
                _jsonSerializer.Serialize(jsonWriter, activity);
                streamWriter.Flush();
                memoryStream.Seek(0, SeekOrigin.Begin);
                await blobReference.UploadAsync(memoryStream, overwrite: overwrite).ConfigureAwait(false);
            }

            var metaData = new Dictionary<string, string>
            {
                ["Id"] = activity.Id,
                ["FromId"] = activity.From?.Id,
                ["RecipientId"] = activity.Recipient?.Id,
                ["Timestamp"] = activity.Timestamp.Value.ToString("O")
            };
            await blobReference.SetMetadataAsync(metaData).ConfigureAwait(false);
        }

        private string GetBlobName(IActivity activity)
        {
            var blobName = $"{SanitizeKey(activity.ChannelId)}/{SanitizeKey(activity.Conversation.Id)}/{SanitizeKey(activity.Id)}-{activity.Timestamp.Value.Ticks.ToString("x")}.json";
            return blobName;
        }

        private string SanitizeKey(string key)
        {
            // Blob Name rules: case-sensitive any url char
            return Uri.EscapeDataString(key);
        }
    }
}
