// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Represents a transcript logger that writes activities to a <see cref="Trace"/> object.
    /// </summary>
    public class TraceTranscriptLogger : ITranscriptLogger
    {
        private static readonly JsonSerializerSettings _serializationSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };

        private readonly bool _traceActivity;

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceTranscriptLogger"/> class.
        /// </summary>
        public TraceTranscriptLogger()
            : this(true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceTranscriptLogger"/> class.
        /// </summary>
        /// <param name="traceActivity">Indicates if trace information should be logged.</param>
        public TraceTranscriptLogger(bool traceActivity)
        {
            this._traceActivity = traceActivity;
        }

        /// <summary>
        /// Log an activity to the transcript.
        /// </summary>
        /// <param name="activity">The activity to transcribe.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public Task LogActivityAsync(IActivity activity)
        {
            BotAssert.ActivityNotNull(activity);
            if (_traceActivity)
            {
                Trace.TraceInformation(JsonConvert.SerializeObject(activity, _serializationSettings));
            }
            else
            {
                if (Debugger.IsAttached && activity.Type == ActivityTypes.Message)
                {
                    Trace.TraceInformation($"{activity.From.Name ?? activity.From.Id ?? activity.From.Role} [{activity.Type}] {activity.AsMessageActivity()?.Text}");
                }
                else
                {
                    Trace.TraceInformation($"{activity.From.Name ?? activity.From.Id ?? activity.From.Role} [{activity.Type}]");
                }
            }

            return Task.CompletedTask;
        }
    }
}
