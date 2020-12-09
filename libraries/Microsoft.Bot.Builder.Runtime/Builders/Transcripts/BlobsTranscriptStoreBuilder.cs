// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Azure.Blobs;
using Microsoft.Bot.Builder.Runtime.Extensions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Runtime.Builders.Transcripts
{
    /// <summary>
    /// Defines an implementation of <see cref="ITranscriptLoggerBuilder"/> that returns an instance
    /// of <see cref="BlobsTranscriptStore"/>.
    /// </summary>
    [JsonObject]
    internal class BlobsTranscriptStoreBuilder : ITranscriptLoggerBuilder
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.BlobsTranscriptStore";

        /// <summary>
        /// Gets or sets the connection string used to connect to Azure Blob Storage.
        /// </summary>
        /// <value>
        /// The connection string used to connect to Azure Blob Storage.
        /// </value>
        [JsonProperty("connectionString")]
        public StringExpression ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the name of the container where transcript blobs will be stored.
        /// </summary>
        /// <value>
        /// The name of the container where transcript blobs will be stored.
        /// </value>
        [JsonProperty("containerName")]
        public StringExpression ContainerName { get; set; }

        /// <summary>
        /// Builds an instance of type <see cref="BlobsTranscriptStore"/>.
        /// </summary>
        /// <param name="services">
        /// Provider containing all services registered with the application's service collection.
        /// </param>
        /// <param name="configuration">Application configuration.</param>
        /// <returns>An instance of type <see cref="BlobsTranscriptStore"/>.</returns>
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

            return new BlobsTranscriptStore(
                dataConnectionString: this.ConnectionString.GetConfigurationValue(configuration),
                containerName: this.ContainerName.GetConfigurationValue(configuration));
        }
    }
}
