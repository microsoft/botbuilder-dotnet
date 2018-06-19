// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
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

        private const int PageSize = 20;

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
                await SaveActivityAsync(activity, activityPath);
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(activityPath));
                await SaveActivityAsync(activity, activityPath);
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
        public async Task<PagedResult<IActivity>> GetTranscriptActivities(string channelId, string conversationId, string continuationToken = null, DateTime startDate = default(DateTime))
        {
            if (channelId == null)
                throw new ArgumentNullException($"missing {nameof(channelId)}");

            if (conversationId == null)
                throw new ArgumentNullException($"missing {nameof(conversationId)}");

            var pagedResult = new PagedResult<IActivity>();
            var transcriptPath = GetTranscriptFolder(channelId, conversationId);
            DirectoryInfo dir = new DirectoryInfo(transcriptPath);

            if (dir.Exists)
            {
                IEnumerable<FileInfo> files;
                string lastFile = null;
                if (continuationToken != null)
                {
                    files = dir.EnumerateFiles("*.json")
                        .OrderBy(fi => fi.CreationTimeUtc)
                        .Where(fi => fi.CreationTimeUtc >= startDate)
                        .SkipWhile(fi => fi.Name != continuationToken)
                        .Skip(1)
                        .Take(PageSize);
                }
                else
                {
                    files = dir.EnumerateFiles("*.json")
                        .OrderBy(fi => fi.CreationTimeUtc)
                        .Where(fi => fi.CreationTimeUtc >= startDate)
                        .Take(PageSize);
                }

                List<Task<Activity>> loadTasks = new List<Task<Activity>>();
                foreach (var fi in files)
                {
                    loadTasks.Add(LoadActivityAsync(fi.FullName));
                    lastFile = fi.Name;
                }

                await Task.WhenAll(loadTasks).ConfigureAwait(false);

                pagedResult.Items = loadTasks.Select(t => t.Result).ToArray();
                if (pagedResult.Items.Length == PageSize)
                    pagedResult.ContinuationToken = lastFile;

            }
            return pagedResult;
        }

        /// <summary>
        /// Delete a conversation
        /// </summary>
        /// <param name="channelId">channelid for the conversation</param>
        /// <param name="conversationId">conversation id</param>
        /// <returns></returns>
        public Task DeleteTranscript(string channelId, string conversationId)
        {
            if (channelId == null)
                throw new ArgumentNullException($"{nameof(channelId)} should not be null");

            if (conversationId == null)
                throw new ArgumentNullException($"{nameof(conversationId)} should not be null");

            var transcriptFolder = GetTranscriptFolder(channelId, conversationId);
            Directory.Delete(transcriptFolder, true);
            return Task.CompletedTask;
        }

        /// <summary>
        /// List conversations
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="continuationToken"></param>
        /// <returns></returns>
        public async Task<PagedResult<Transcript>> ListTranscripts(string channelId, string continuationToken = null)
        {
            if (channelId == null)
                throw new ArgumentNullException($"missing {nameof(channelId)}");

            var pagedResult = new PagedResult<Transcript>();
            var conversationPath = GetChannelFolder(channelId);
            DirectoryInfo dir = new DirectoryInfo(conversationPath);

            if (dir.Exists)
            {
                DirectoryInfo[] transcriptFolders;

                if (continuationToken != null)
                {
                    transcriptFolders = dir.EnumerateDirectories()
                        .SkipWhile(di => di.Name != continuationToken)
                        .Skip(1)
                        .Take(PageSize)
                        .ToArray();
                }
                else
                {
                    transcriptFolders = dir.EnumerateDirectories()
                        .Take(20)
                        .ToArray();
                }
                if (transcriptFolders.Length == PageSize)
                    pagedResult.ContinuationToken = transcriptFolders.Last().Name;

                pagedResult.Items = transcriptFolders.Select(di => new Transcript()
                {
                    ChannelId = channelId,
                    Id = di.Name
                }).ToArray();

            }
            return pagedResult;
        }


        private async Task<Activity> LoadActivityAsync(string path)
        {
            using (TextReader reader = File.OpenText(path))
            {
                string json = await reader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<Activity>(json);
            }
        }

        private async Task SaveActivityAsync(IActivity activity, string activityPath)
        {
            using (TextWriter writer = new StreamWriter(File.OpenWrite(activityPath), Encoding.UTF8))
            {
                await writer.WriteAsync(JsonConvert.SerializeObject(activity, jsonSettings));
            }
        }


        private string GetActivityPath(IActivity activity)
        {
            return GetActivityPath(activity.ChannelId, activity.Conversation.Id, activity.Id, activity.Timestamp ?? default(DateTimeOffset));
        }

        private string GetActivityPath(string channelId, string conversationId, string activityId, DateTimeOffset timestamp)
        {
            var conversationFolder = GetTranscriptFolder(channelId, conversationId);

            string fileName = $"{timestamp.Ticks.ToString("x")}-{SanitizeKey(activityId)}.json";
            return Path.Combine(conversationFolder, fileName);
        }

        private string GetChannelFolder(string channelId)
        {
            return Path.Combine(this.rootFolder, SanitizeKey(channelId));
        }

        private string GetTranscriptFolder(string channelId, string conversationId)
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
