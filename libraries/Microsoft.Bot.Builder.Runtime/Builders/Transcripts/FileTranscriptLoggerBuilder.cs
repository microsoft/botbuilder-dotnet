// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Runtime.Extensions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Runtime.Builders.Transcripts
{
    /// <summary>
    /// Defines an implementation of <see cref="ITranscriptLoggerBuilder"/> that returns an instance
    /// of <see cref="FileTranscriptLogger"/>.
    /// </summary>
    [JsonObject]
    public class FileTranscriptLoggerBuilder : ITranscriptLoggerBuilder<FileTranscriptLogger>
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.FileTranscriptLogger";

        /// <summary>
        /// Gets or sets the folder to write transcript files to. Defaults to the current directory.
        /// </summary>
        /// <value>
        /// The folder to write transcript files to. Defaults to the current directory.
        /// </value>
        [JsonProperty("folder")]
        public StringExpression Folder { get; set; }

        /// <summary>
        /// Gets or sets whether to overwrite existing transcript files or not. Defaults to true.
        /// </summary>
        /// <value>
        /// Indicates whether to overwrite existing transcript files or not. Defaults to true.
        /// </value>
        [JsonProperty("unitTestMode")]
        public BoolExpression UnitTestMode { get; set; }

        /// <summary>
        /// Builds an instance of type <see cref="FileTranscriptLogger"/>.
        /// </summary>
        /// <param name="services">
        /// Provider containing all services registered with the application's service collection.
        /// </param>
        /// <param name="configuration">Application configuration.</param>
        /// <returns>An instance of type <see cref="FileTranscriptLogger"/>.</returns>
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
