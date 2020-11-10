// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Microsoft.Bot.Runtime.Builders.Transcripts
{
    // TODO #39: Change parent interface to ITranscriptStoreBuilder
    [JsonObject]
    public class MemoryTranscriptStoreBuilder : ITranscriptLoggerBuilder
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.MemoryTranscriptStore";

        public ITranscriptLogger Build(IServiceProvider services, IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            return new MemoryTranscriptStore();
        }
    }
}
