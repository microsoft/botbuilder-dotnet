// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// FileTranscriptLogger which creates a .transcript file for each conversationId.
    /// </summary>
    /// <remarks>
    /// This is a useful class for unit tests.
    /// </remarks>
    public class FileTranscriptLogger : ITranscriptStore
    {
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };

        private readonly string _folder;
        private readonly bool _unitTestMode;
        private readonly HashSet<string> _started = new HashSet<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTranscriptLogger"/> class.
        /// </summary>
        /// <param name="folder">folder to place the transcript files (Default current folder).</param>
        /// <param name="unitTestMode">unitTestMode will overwrite transcript files.</param>
        public FileTranscriptLogger(string folder = null, bool unitTestMode = true)
        {
            if (folder == null)
            {
                folder = Environment.CurrentDirectory;
            }

            folder = PathUtils.NormalizePath(folder);

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            this._folder = folder;
            this._unitTestMode = unitTestMode;
        }

        /// <summary>
        /// Log an activity to the transcript.
        /// </summary>
        /// <param name="activity">The activity to transcribe.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task LogActivityAsync(IActivity activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            var transcriptFile = GetTranscriptFile(activity.ChannelId, activity.Conversation.Id);

            if (Debugger.IsAttached && activity.Type == ActivityTypes.Message)
            {
                System.Diagnostics.Trace.TraceInformation($"{activity.From.Name ?? activity.From.Id ?? activity.From.Role} [{activity.Type}] {activity.AsMessageActivity()?.Text}");
            }
            else
            {
                System.Diagnostics.Trace.TraceInformation($"{activity.From.Name ?? activity.From.Id ?? activity.From.Role} [{activity.Type}]");
            }

            // try 3 times
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    if ((this._unitTestMode == true && !_started.Contains(transcriptFile)) || !File.Exists(transcriptFile))
                    {
                        Trace.TraceInformation($"file://{transcriptFile.Replace("\\", "/")}");
                        _started.Add(transcriptFile);

                        using (var stream = File.OpenWrite(transcriptFile))
                        {
                            using (var writer = new StreamWriter(stream) as TextWriter)
                            {
                                await writer.WriteAsync($"[{JsonConvert.SerializeObject(activity, _jsonSettings)}]").ConfigureAwait(false);
                                return;
                            }
                        }
                    }

                    switch (activity.Type)
                    {
                        case ActivityTypes.MessageDelete:
                            await MessageDeleteAsync(activity, transcriptFile).ConfigureAwait(false);
                            return;

                        case ActivityTypes.MessageUpdate:
                            await MessageUpdateAsync(activity, transcriptFile).ConfigureAwait(false);
                            return;

                        default:
                            // append
                            await LogActivityAsync(activity, transcriptFile).ConfigureAwait(false);
                            return;
                    }
                }
#pragma warning disable CA1031 // Do not catch general exception types (we ignore the exception and we retry)
                catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    // try again
                    Trace.TraceError($"Try {i + 1} - Failed to log activity because: {e.GetType()} : {e.Message}");
                }
            }
        }

        /// <summary>
        /// Gets from the store activities that match a set of criteria.
        /// </summary>
        /// <param name="channelId">The ID of the channel the conversation is in.</param>
        /// <param name="conversationId">The ID of the conversation.</param>
        /// <param name="continuationToken">The continuation token (if available).</param>
        /// <param name="startDate">A cutoff date. Activities older than this date are not included.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task completes successfully, the result contains the matching activities.</remarks>
        public async Task<PagedResult<IActivity>> GetTranscriptActivitiesAsync(string channelId, string conversationId, string continuationToken = null, DateTimeOffset startDate = default(DateTimeOffset))
        {
            var transcriptFile = GetTranscriptFile(channelId, conversationId);

            var transcript = await LoadTranscriptAsync(transcriptFile).ConfigureAwait(false);
            var result = new PagedResult<IActivity>();
            result.ContinuationToken = null;
            result.Items = transcript.Where(activity => activity.Timestamp >= startDate).Cast<IActivity>().ToArray();
            return result;
        }

        /// <summary>
        /// Gets the conversations on a channel from the store.
        /// </summary>
        /// <param name="channelId">The ID of the channel.</param>
        /// <param name="continuationToken">Continuation token (if available).</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>List all transcripts for given ChannelID.</remarks>
        public Task<PagedResult<TranscriptInfo>> ListTranscriptsAsync(string channelId, string continuationToken = null)
        {
            List<TranscriptInfo> transcripts = new List<TranscriptInfo>();
            var channelFolder = GetChannelFolder(channelId);

            foreach (var file in Directory.EnumerateFiles(channelFolder, "*.transcript"))
            {
                transcripts.Add(new TranscriptInfo()
                {
                    ChannelId = channelId,
                    Id = Path.GetFileNameWithoutExtension(file),
                    Created = File.GetCreationTime(file),
                });
            }

            return Task.FromResult(new PagedResult<TranscriptInfo>()
            {
                Items = transcripts.ToArray(),
                ContinuationToken = null,
            });
        }

        /// <summary>
        /// Deletes conversation data from the store.
        /// </summary>
        /// <param name="channelId">The ID of the channel the conversation is in.</param>
        /// <param name="conversationId">The ID of the conversation to delete.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public Task DeleteTranscriptAsync(string channelId, string conversationId)
        {
            var transcriptFile = GetTranscriptFile(channelId, conversationId);
            File.Delete(transcriptFile);
            return Task.CompletedTask;
        }

        private static async Task<Activity[]> LoadTranscriptAsync(string transcriptFile)
        {
            if (File.Exists(transcriptFile))
            {
                using (var stream = File.OpenRead(transcriptFile))
                {
                    using (var reader = new StreamReader(stream) as TextReader)
                    {
                        var json = await reader.ReadToEndAsync().ConfigureAwait(false);
                        return JsonConvert.DeserializeObject<Activity[]>(json);
                    }
                }
            }

            return Array.Empty<Activity>();
        }

        private static async Task LogActivityAsync(IActivity activity, string transcriptFile)
        {
            var json = $",\n{JsonConvert.SerializeObject(activity, _jsonSettings)}]";

            using (var stream = File.Open(transcriptFile, FileMode.OpenOrCreate))
            {
                if (stream.Length > 0)
                {
                    stream.Seek(-1, SeekOrigin.End);
                }

                using (TextWriter writer = new StreamWriter(stream))
                {
                    await writer.WriteAsync(json).ConfigureAwait(false);
                }
            }
        }

        private static async Task MessageUpdateAsync(IActivity activity, string transcriptFile)
        {
            // load all activities
            var transcript = await LoadTranscriptAsync(transcriptFile).ConfigureAwait(false);

            for (int i = 0; i < transcript.Length; i++)
            {
                var originalActivity = transcript[i];
                if (originalActivity.Id == activity.Id)
                {
                    var updatedActivity = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(activity));
                    updatedActivity.Type = originalActivity.Type; // fixup original type (should be Message)
                    updatedActivity.LocalTimestamp = originalActivity.LocalTimestamp;
                    updatedActivity.Timestamp = originalActivity.Timestamp;
                    transcript[i] = updatedActivity;
                    var json = JsonConvert.SerializeObject(transcript, _jsonSettings);
                    using (var stream = File.OpenWrite(transcriptFile))
                    {
                        using (var writer = new StreamWriter(stream) as TextWriter)
                        {
                            await writer.WriteAsync(json).ConfigureAwait(false);
                            return;
                        }
                    }
                }
            }
        }

        private static async Task MessageDeleteAsync(IActivity activity, string transcriptFile)
        {
            // load all activities
            var transcript = await LoadTranscriptAsync(transcriptFile).ConfigureAwait(false);

            // if message delete comes in, delete the message from the transcript
            for (int index = 0; index < transcript.Length; index++)
            {
                var originalActivity = transcript[index];
                if (originalActivity.Id == activity.Id)
                {
                    // tombstone the original message
                    transcript[index] = new Activity()
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
                    var json = JsonConvert.SerializeObject(transcript, _jsonSettings);
                    using (var stream = File.OpenWrite(transcriptFile))
                    {
                        using (var writer = new StreamWriter(stream) as TextWriter)
                        {
                            await writer.WriteAsync(json).ConfigureAwait(false);
                            return;
                        }
                    }
                }
            }
        }

        private static string SanitizeString(string str, char[] invalidChars)
        {
            var sb = new StringBuilder(str);

            foreach (var invalidChar in invalidChars)
            {
                sb.Replace(invalidChar.ToString(), string.Empty);
            }

            return sb.ToString();
        }

        private string GetTranscriptFile(string channelId, string conversationId)
        {
            if (channelId == null)
            {
                throw new ArgumentNullException(channelId);
            }

            if (conversationId == null)
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            var channelFolder = GetChannelFolder(channelId);

            var fileName = SanitizeString(conversationId, Path.GetInvalidFileNameChars());

            return Path.Combine(channelFolder, fileName + ".transcript");
        }

        private string GetChannelFolder(string channelId)
        {
            if (channelId == null)
            {
                throw new ArgumentNullException(channelId);
            }

            var folderName = SanitizeString(channelId, Path.GetInvalidPathChars());

            var channelFolder = Path.Combine(_folder, folderName);
            if (!Directory.Exists(channelFolder))
            {
                Directory.CreateDirectory(channelFolder);
            }

            return channelFolder;
        }
    }
}
