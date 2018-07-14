// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Representas a transcript logger that writes activites to a <see cref="Trace"/> object.
    /// </summary>
    public class TraceTranscriptLogger : ITranscriptLogger
    {
        private static JsonSerializerSettings serializationSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };

        /// <summary>
        /// Log an activity to the transcript.
        /// </summary>
        /// <param name="activity">The activity to transcribe.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public Task LogActivityAsync(IActivity activity)
        {
            BotAssert.ActivityNotNull(activity);
            Trace.TraceInformation(JsonConvert.SerializeObject(activity, serializationSettings));
            return Task.CompletedTask;
        }
    }
}
