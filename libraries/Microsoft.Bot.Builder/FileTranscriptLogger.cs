// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs
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
                if (activity.Type == ActivityTypes.Message)
                {
                    System.Diagnostics.Trace.TraceInformation($"{activity.From.Name ?? activity.From.Id ?? activity.From.Role}: {((Activity)activity).Text}");
                }
                else
                {
                    System.Diagnostics.Trace.TraceInformation($"{activity.From.Name ?? activity.From.Id ?? activity.From.Role} [{activity.Type}]");
                }

                string transcriptFile = Path.Combine(folder, activity.Conversation.Id + ".transcript");

                List<Activity> transcript = null;

                if (this.unitTestMode == true && !started.Contains(transcriptFile))
                {
                    started.Add(transcriptFile);
                    File.Delete(transcriptFile);
                }

                if (File.Exists(transcriptFile))
                {
                    transcript = JsonConvert.DeserializeObject<List<Activity>>(File.ReadAllText(transcriptFile));
                }

                if (transcript == null)
                {
                    transcript = new List<Activity>();
                    System.Diagnostics.Trace.TraceInformation($"Transcript file is: file://{transcriptFile.Replace("\\", "/")}");
                }

                transcript.Add((Activity)activity);
                File.WriteAllText(transcriptFile, JsonConvert.SerializeObject(transcript, jsonSettings));
            }
        }
    }
}

