using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Azure
{
    public class AzureBlobTranscriptStore : ITranscriptStore
    {
        private readonly static JsonSerializer jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        });

        private static HashSet<string> _checkedContainers = new HashSet<string>();


        /// <summary>
        /// The Azure Storage Blob Container where entities will be stored
        /// </summary>
        public Lazy<CloudBlobContainer> Container { get; private set; }


        public AzureBlobTranscriptStore(string dataConnectionString, string containerName)
            : this(CloudStorageAccount.Parse(dataConnectionString), containerName)
        {

        }

        public AzureBlobTranscriptStore(CloudStorageAccount storageAccount, string containerName)
        {
            if (storageAccount == null) throw new ArgumentNullException(nameof(storageAccount));

            this.Container = new Lazy<CloudBlobContainer>(() =>
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

        public async Task LogActivity(IActivity activity)
        {
            if (activity == null)
                throw new ArgumentNullException("activity cannot be null for LogActivity()");

            var blobName = GetBlobName(activity);
            var blobReference = this.Container.Value.GetBlockBlobReference(blobName);
            blobReference.Properties.ContentType = "application/json";
            blobReference.Metadata["FromId"] = activity.From.Id;
            blobReference.Metadata["RecipientId"] = activity.Recipient.Id;
            blobReference.Metadata["Timestamp"] = activity.Timestamp.Value.ToString("O");
            using (var blobStream = await blobReference.OpenWriteAsync())
            {
                using (var jsonWriter = new JsonTextWriter(new StreamWriter(blobStream)))
                {
                    jsonSerializer.Serialize(jsonWriter, activity);
                }
            }
            await blobReference.SetMetadataAsync();
        }

        public async Task<PagedResult<IActivity>> GetConversationActivities(string channelId, string conversationId, string continuationToken = null, DateTime startDate = default(DateTime))
        {
            if (channelId == null)
                throw new ArgumentNullException($"missing {nameof(channelId)}");

            if (conversationId == null)
                throw new ArgumentNullException($"missing {nameof(conversationId)}");

            var pagedResult = new PagedResult<IActivity>();

            var dirName = GetDirName(channelId, conversationId);
            var dir = this.Container.Value.GetDirectoryReference(dirName);
            int pageSize = 20;
            BlobContinuationToken token = null;
            List<CloudBlockBlob> blobs = new List<CloudBlockBlob>();
            do
            {
                var segment = await dir.ListBlobsSegmentedAsync(false, BlobListingDetails.Metadata, null, token, null, null);

                foreach (var blob in segment.Results.Cast<CloudBlockBlob>())
                {
                    if (DateTime.Parse(blob.Metadata["Timestamp"]).ToUniversalTime() >= startDate)
                    {
                        if (continuationToken != null)
                        {
                            if (blob.Name == continuationToken)
                                // we found continuation token 
                                continuationToken = null;
                            // skip record
                        }
                        else
                        {
                            blobs.Add(blob);
                            if (blobs.Count == pageSize)
                                break;
                        }
                    }
                }

                if (segment.ContinuationToken != null)
                    token = segment.ContinuationToken;
            } while (token != null && blobs.Count < pageSize);

            pagedResult.Items = blobs
                .Select(async bl =>
                {
                    var json = await bl.DownloadTextAsync();
                    return JsonConvert.DeserializeObject<Activity>(json);
                })
                .Select(t => t.Result)
                .ToArray();

            if (pagedResult.Items.Length == pageSize)
                pagedResult.ContinuationToken = blobs.Last().Name;

            return pagedResult;
        }

        public async Task<PagedResult<Conversation>> ListConversations(string channelId, string continuationToken = null)
        {
            if (channelId == null)
                throw new ArgumentNullException($"missing {nameof(channelId)}");

            var dirName = GetDirName(channelId);
            var dir = this.Container.Value.GetDirectoryReference(dirName);
            int pageSize = 20;
            BlobContinuationToken token = null;
            List<Conversation> conversations = new List<Conversation>();
            do
            {
                var segment = await dir.ListBlobsSegmentedAsync(false, BlobListingDetails.Metadata, null, token, null, null);

                foreach (var blob in segment.Results.Where(c => c is CloudBlobDirectory).Cast<CloudBlobDirectory>())
                {
                    var conversation = new Conversation() { Id = blob.Prefix.Split('/').Where(s => s.Length > 0).Last(), ChannelId = channelId };
                    if (continuationToken != null)
                    {
                        if (conversation.Id == continuationToken)
                            // we found continuation token 
                            continuationToken = null;
                        // skip record
                    }
                    else
                    {
                        conversations.Add(conversation);
                        if (conversations.Count == pageSize)
                            break;
                    }
                }

                if (segment.ContinuationToken != null)
                    token = segment.ContinuationToken;
            } while (token != null && conversations.Count < pageSize);

            var pagedResult = new PagedResult<Conversation>();
            pagedResult.Items = conversations.ToArray();

            if (pagedResult.Items.Length == 20)
                pagedResult.ContinuationToken = pagedResult.Items.Last().Id;

            return pagedResult;
        }

        public async Task DeleteConversation(string channelId, string conversationId)
        {
            if (channelId == null)
                throw new ArgumentNullException($"{nameof(channelId)} should not be null");

            if (conversationId == null)
                throw new ArgumentNullException($"{nameof(conversationId)} should not be null");

            var dirName = GetDirName(channelId, conversationId);
            var dir = this.Container.Value.GetDirectoryReference(dirName);
            BlobContinuationToken token = null;
            List<CloudBlockBlob> blobs = new List<CloudBlockBlob>();
            do
            {
                var segment = await dir.ListBlobsSegmentedAsync(false, BlobListingDetails.None, null, token, null, null);
                foreach (var blob in segment.Results.Cast<CloudBlockBlob>())
                {
                    await blob.DeleteIfExistsAsync();
                }
                if (segment.ContinuationToken != null)
                    token = segment.ContinuationToken;
            } while (token != null);
        }

        /// <summary>
        /// Get a blob name validated representation of an entity
        /// </summary>
        /// <param name="key">The key used to identify the entity</param>
        /// <returns></returns>
        private string GetBlobName(IActivity activity)
        {
            var blobName = $"{SanitizeKey(activity.ChannelId)}/{SanitizeKey(activity.Conversation.Id)}/{activity.Timestamp.Value.Ticks.ToString("x")}-{SanitizeKey(activity.Id)}.json";
            NameValidator.ValidateBlobName(blobName);
            return blobName;
        }

        private string GetDirName(string channelId, string conversationId = null)
        {
            string dirName = "";
            if (conversationId != null)
                dirName = $"{SanitizeKey(channelId)}/{SanitizeKey(conversationId)}";
            else
                dirName = $"{SanitizeKey(channelId)}";
            // NameValidator.ValidateDirectoryName(dirName);
            return dirName;
        }

        private static Lazy<Dictionary<char, string>> badChars = new Lazy<Dictionary<char, string>>(() =>
        {
            char[] badChars = new char[] { '\\', '?', '/', '#', '\t', '\n', '\r' };
            var dict = new Dictionary<char, string>();
            foreach (var badChar in badChars)
                dict[badChar] = '%' + ((int)badChar).ToString("x2");
            return dict;
        });

        private string SanitizeKey(string key)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char ch in key)
            {
                if (badChars.Value.TryGetValue(ch, out string val))
                    sb.Append(val);
                else
                    sb.Append(ch);
            }
            return sb.ToString();
        }

    }
}
