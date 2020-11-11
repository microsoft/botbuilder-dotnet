// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Runtime.Builders.Transcripts;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Runtime.Builders.Middleware
{
    [JsonObject]
    public class TranscriptLoggerMiddlewareBuilder : IMiddlewareBuilder
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.TranscriptLoggerMiddleware";

        [JsonProperty("transcriptStore")]
        public ITranscriptLoggerBuilder TranscriptStore { get; set; }

        public IMiddleware Build(IServiceProvider services, IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            return new TranscriptLoggerMiddleware(
                transcriptLogger: this.TranscriptStore?.Build(services, configuration));
        }
    }
}
