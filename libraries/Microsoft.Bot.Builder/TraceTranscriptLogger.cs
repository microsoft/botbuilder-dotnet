// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// TraceTranscriptLogger, writes activites to System.Diagnostics.Trace
    /// </summary>
    public class TraceTranscriptLogger : ITranscriptLogger
    {
        private static JsonSerializerSettings serializationSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };

        /// <summary>
        /// Log an activity to the transcript
        /// </summary>
        /// <param name="activity">activity to log</param>
        /// <returns></returns>
        public Task LogActivity(IActivity activity)
        {
            BotAssert.ActivityNotNull(activity);
            Trace.TraceInformation(JsonConvert.SerializeObject(activity, serializationSettings));
            return Task.CompletedTask;
        }
    }
}