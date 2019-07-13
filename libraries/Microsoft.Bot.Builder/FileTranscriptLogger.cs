// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// FileTranscriptLogger which creates a .transcript file for each conversationId
    /// </summary>
    /// <remarks>
    /// This is a useful class for unit tests
    /// </remarks>
    public class FileTranscriptLogger : ITranscriptLogger
    {
        private static JsonSerializerSettings jsonSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };

        private string folder;
        private bool unitTestMode;
        private HashSet<string> started = new HashSet<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTranscriptLogger"/> class.
        /// </summary>
        /// <param name="folder">folder to place the transcript files (Default current folder)</param>
        /// <param name="unitTestMode">unitTestMode will overwrite transcript files</param>
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

            this.folder = folder;
            this.unitTestMode = unitTestMode;
        }

        /// <summary>
        /// Log an activity to the transcript.
        /// </summary>
        /// <param name="activity">The activity to transcribe.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task LogActivityAsync(IActivity activity)
        {
            if (activity != null)
            {
                var json = JsonConvert.SerializeObject(activity, jsonSettings);
                string transcriptFile = Path.Combine(folder, activity.Conversation.Id + ".transcript");

                if ((this.unitTestMode == true && !started.Contains(transcriptFile)) || !File.Exists(transcriptFile))
                {
                    System.Diagnostics.Trace.TraceInformation($"file://{transcriptFile.Replace("\\", "/")}");
                    started.Add(transcriptFile);
                    json = $"[{json}]";
                    File.Delete(transcriptFile);
                }
                else
                {
                    json = $",\n{json}]";
                }

                if (activity.Type == ActivityTypes.Message)
                {
                    System.Diagnostics.Trace.TraceInformation($"{activity.From.Name ?? activity.From.Id ?? activity.From.Role}: {((Activity)activity).Text}");
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

                        return;
                    }
                    catch (Exception)
                    {
                        // try again
                    }
                }
            }
        }
    }
}
