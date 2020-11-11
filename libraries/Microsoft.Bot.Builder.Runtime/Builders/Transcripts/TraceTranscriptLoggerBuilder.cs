// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Runtime.Extensions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Runtime.Builders.Transcripts
{
    [JsonObject]
    public class TraceTranscriptLoggerBuilder : ITranscriptLoggerBuilder<TraceTranscriptLogger>
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.TraceTranscriptLogger";

        [JsonProperty("traceActivity")]
        public BoolExpression TraceActivity { get; set; }

        public TraceTranscriptLogger Build(IServiceProvider services, IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            return new TraceTranscriptLogger(
                traceActivity: this.TraceActivity?.GetConfigurationValue(configuration) ?? true);
        }
    }
}
