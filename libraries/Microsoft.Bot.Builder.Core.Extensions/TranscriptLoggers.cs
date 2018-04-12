using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Core.Extensions
{
    /// <summary>
    /// TraceTranscriptLogger, writes activites to Trace output
    /// </summary>
    public class TraceTranscriptLogger : ITranscriptLogger
    {
        private static JsonSerializerSettings serializationSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };

        /// <summary>
        /// Log an activity to the transcript
        /// </summary>
        /// <param name="activity">activity to log</param>
        /// <returns></returns>
        public async Task LogActivity(IActivity activity)
        {
            System.Diagnostics.Trace.TraceInformation(JsonConvert.SerializeObject(activity, serializationSettings));
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
            Console.WriteLine(JsonConvert.SerializeObject(activity, serializationSettings));
        }
    }

}