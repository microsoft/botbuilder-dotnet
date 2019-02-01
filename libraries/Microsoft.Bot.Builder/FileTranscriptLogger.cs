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
    /// FileTranscriptLogger which creates a .transcript file for each conversationId.
    /// </summary>
    /// <remarks>
    /// This is a useful class for unit tests.  It is not meant to be used as a general purpose file based transcript logger as it will not scale to large conversations.
    /// </remarks>
    public class FileTranscriptLogger : ITranscriptLogger
    {

        private static JsonSerializerSettings jsonSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };

        private string _folder;
        private bool _unitTestMode;
        private Dictionary<string, object> _started = new Dictionary<string, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTranscriptLogger"/> class.
        /// </summary>
        /// <param name="_folder">folder to place the transcript files (Default current folder)</param>
        /// <param name="unitTestMode">unitTestMode will overwrite transcript files</param>
        public FileTranscriptLogger(string folder = null, bool unitTestMode = true)
        {
            this._unitTestMode = unitTestMode;
            if (folder == null)
            {
                folder = Environment.CurrentDirectory;
            }
            this._folder = folder;

            if (!Directory.Exists(_folder))
            {
                Directory.CreateDirectory(_folder);
            }

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
                string transcriptFile = Path.Combine(_folder, activity.Conversation.Id + ".transcript");

                List<Activity> transcript = null;

                lock (_started)
                {
                    if (this._unitTestMode == true && !_started.ContainsKey(transcriptFile))
                    {
                        _started.Add(transcriptFile, new object());
                        File.Delete(transcriptFile);
                    }
                }

                lock (_started[transcriptFile])
                {
                    if (File.Exists(transcriptFile))
                    {
                        transcript = JsonConvert.DeserializeObject<List<Activity>>(File.ReadAllText(transcriptFile));
                    }

                    if (transcript == null)
                    {
                        transcript = new List<Activity>();
                        System.Diagnostics.Trace.TraceInformation($"file://{transcriptFile.Replace("\\", "/")}");
                    }

                    if (activity.Type == ActivityTypes.Message)
                    {
                        System.Diagnostics.Trace.TraceInformation($"{activity.From.Name ?? activity.From.Id ?? activity.From.Role}: {((Activity)activity).Text}");
                    }
                    else
                    {
                        System.Diagnostics.Trace.TraceInformation($"{activity.From.Name ?? activity.From.Id ?? activity.From.Role} [{activity.Type}]");
                    }

                    transcript.Add((Activity)activity);
                    File.WriteAllText(transcriptFile, JsonConvert.SerializeObject(transcript, jsonSettings));
                }
            }
        }
    }
}
