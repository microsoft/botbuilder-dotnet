using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Core.Extensions
{
    /// <summary>
    /// The file transcript store stores transcripts in file system with each activity as a file.
    /// </summary>
    public class FileTranscriptStore : ITranscriptStore
    {
        private static JsonSerializerSettings jsonSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };
        protected string rootFolder;

        public FileTranscriptStore(string folder)
        {
            this.rootFolder = folder ?? throw new ArgumentNullException("Missing folder");
        }

        /// <summary>
        /// Log an activity to the transcript
        /// </summary>
        /// <param name="activity">activity to log</param>
        /// <returns></returns>
        public async Task LogActivity(IActivity activity)
        {
            if (activity == null)
                throw new ArgumentNullException("activity cannot be null for LogActivity()");

            var activityPath = GetActivityPath(activity);

            try
            {
                File.WriteAllText(activityPath, JsonConvert.SerializeObject(activity, jsonSettings));
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(activityPath));
                File.WriteAllText(activityPath, JsonConvert.SerializeObject(activity, jsonSettings));
            }
            File.SetCreationTimeUtc(activityPath, activity.Timestamp.Value.UtcDateTime);
        }

        /// <summary>
        /// Get activity records for conversationId
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="conversationId"></param>
        /// <param name="continuationToken"></param>
        /// <param name="startDate"></param>
        /// <returns></returns>
        public async Task<PagedResult<IActivity>> GetConversationActivities(string channelId, string conversationId, string continuationToken = null, DateTime startDate = default(DateTime))
        {
            if (channelId == null)
                throw new ArgumentNullException($"missing {nameof(channelId)}");

            if (conversationId == null)
                throw new ArgumentNullException($"missing {nameof(conversationId)}");

            var pagedResult = new PagedResult<IActivity>();
            var conversationPath = GetConversationFolder(channelId, conversationId);
            DirectoryInfo dir = new DirectoryInfo(conversationPath);

            if (dir.Exists)
            {
                FileInfo[] files;
                if (continuationToken != null)
                {
                    files = dir.EnumerateFiles("*.json")
                        .OrderBy(fi => fi.CreationTimeUtc)
                        .Where(fi => fi.CreationTimeUtc >= startDate)
                        .SkipWhile(fi => fi.Name != continuationToken)
                        .Skip(1)
                        .Take(20)
                        .ToArray();
                }
                else
                {
                    files = dir.EnumerateFiles("*.json")
                        .OrderBy(fi => fi.CreationTimeUtc)
                        .Where(fi => fi.CreationTimeUtc >= startDate)
                        .Take(20)
                        .ToArray();
                }
                if (files.Length == 20)
                    pagedResult.ContinuationToken = files.Last().Name;

                pagedResult.Items = files.Select(fi => JsonConvert.DeserializeObject<Activity>(File.ReadAllText(fi.FullName))).ToArray();
            }
            return pagedResult;
        }

        /// <summary>
        /// Delete a conversation
        /// </summary>
        /// <param name="channelId">channelid for the conversation</param>
        /// <param name="conversationId">conversation id</param>
        /// <param name="cancel"></param>
        /// <returns></returns>
        public async Task DeleteConversation(string channelId, string conversationId)
        {
            if (channelId == null)
                throw new ArgumentNullException($"{nameof(channelId)} should not be null");

            if (conversationId == null)
                throw new ArgumentNullException($"{nameof(conversationId)} should not be null");

            var conversationDirectory = GetConversationFolder(channelId, conversationId);
            Directory.Delete(conversationDirectory, true);
        }

        /// <summary>
        /// List conversations
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="continuationToken"></param>
        /// <returns></returns>
        public async Task<PagedResult<Conversation>> ListConversations(string channelId, string continuationToken = null)
        {
            if (channelId == null)
                throw new ArgumentNullException($"missing {nameof(channelId)}");

            var pagedResult = new PagedResult<Conversation>();
            var conversationPath = GetChannelFolder(channelId);
            DirectoryInfo dir = new DirectoryInfo(conversationPath);

            if (dir.Exists)
            {
                DirectoryInfo[] conversationFolders;

                if (continuationToken != null)
                {
                    conversationFolders = dir.EnumerateDirectories()
                        .SkipWhile(di => di.Name != continuationToken)
                        .Skip(1)
                        .Take(20)
                        .ToArray();
                }
                else
                {
                    conversationFolders = dir.EnumerateDirectories()
                        .Take(20)
                        .ToArray();
                }
                if (conversationFolders.Length == 20)
                    pagedResult.ContinuationToken = conversationFolders.Last().Name;

                pagedResult.Items = conversationFolders.Select(di => new Conversation()
                {
                    ChannelId = channelId,
                    Id = di.Name
                }).ToArray();

            }
            return pagedResult;
        }



        private string GetActivityPath(IActivity activity)
        {
            return GetActivityPath(activity.ChannelId, activity.Conversation.Id, activity.Id, activity.Timestamp ?? default(DateTimeOffset));
        }

        private string GetActivityPath(string channelId, string conversationId, string activityId, DateTimeOffset timestamp)
        {
            var conversationFolder = GetConversationFolder(channelId, conversationId);

            string fileName = $"{timestamp.Ticks.ToString("x")}-{SanitizeKey(activityId)}.json";
            return Path.Combine(conversationFolder, fileName);
        }

        private string GetChannelFolder(string channelId)
        {
            return Path.Combine(this.rootFolder, SanitizeKey(channelId));
        }

        private string GetConversationFolder(string channelId, string conversationId)
        {
            return Path.Combine(this.rootFolder, Path.Combine(SanitizeKey(channelId), SanitizeKey(conversationId)));
        }

        private static Lazy<Dictionary<char, string>> badChars = new Lazy<Dictionary<char, string>>(() =>
            {
                char[] badChars = Path.GetInvalidFileNameChars();
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
