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
    public class FileTranscriptLoggerBuilder : ITranscriptLoggerBuilder<FileTranscriptLogger>
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.FileTranscriptLogger";

        [JsonProperty("folder")]
        public StringExpression Folder { get; set; }

        [JsonProperty("unitTestMode")]
        public BoolExpression UnitTestMode { get; set; }

        public FileTranscriptLogger Build(IServiceProvider services, IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            return new FileTranscriptLogger(
                folder: this.Folder?.GetConfigurationValue(configuration),
                unitTestMode: this.UnitTestMode?.GetConfigurationValue(configuration) ?? true);
        }
    }
}
