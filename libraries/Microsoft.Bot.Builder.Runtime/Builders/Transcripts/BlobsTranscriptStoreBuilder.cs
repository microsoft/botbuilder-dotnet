// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Runtime.Extensions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Runtime.Builders.Transcripts
{
    // TODO #39: Change parent interface to ITranscriptStoreBuilder
    [JsonObject]
    public class BlobsTranscriptStoreBuilder : ITranscriptLoggerBuilder
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.BlobsTranscriptStore";

        [JsonProperty("connectionString")]
        public StringExpression ConnectionString { get; set; }

        [JsonProperty("containerName")]
        public StringExpression ContainerName { get; set; }

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

            // TODO #40: Change to Microsoft.Bot.Builder.Azure.BlobsTranscriptStore
            return new AzureBlobTranscriptStore(
                dataConnectionstring: this.ConnectionString.GetConfigurationValue(configuration),
                containerName: this.ContainerName.GetConfigurationValue(configuration));
        }
    }
}
