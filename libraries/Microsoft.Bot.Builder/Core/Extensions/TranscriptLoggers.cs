// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Core.Extensions
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
            System.Diagnostics.Trace.TraceInformation(JsonConvert.SerializeObject(activity, serializationSettings));
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// ConsoleTranscriptLogger , writes activites to Console output
    /// </summary>
    public class ConsoleTranscriptLogger : ITranscriptLogger
    {
        private static JsonSerializerSettings serializationSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };

        /// <summary>
        /// Log an activity to the transcript
        /// </summary>
        /// <param name="activity">activity to log</param>
        /// <returns></returns>
        public async Task LogActivity(IActivity activity)
        {
            BotAssert.ActivityNotNull(activity);
            await Console.Out.WriteLineAsync(JsonConvert.SerializeObject(activity, serializationSettings));
        }
    }

}