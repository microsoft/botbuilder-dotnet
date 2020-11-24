// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Runtime.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Runtime.Providers.Storage
{
    /// <summary>
    /// Defines an implementation of <see cref="IStorageProvider"/> that registers
    /// <see cref="AzureBlobStorage"/> with the application's service collection.
    /// </summary>
    [JsonObject]
    public class BlobStorageProvider : IStorageProvider
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.BlobStorage";

        /// <summary>
        /// Gets or sets the connection string used to connect to Azure Blob Storage.
        /// </summary>
        /// <value>
        /// The connection string used to connect to Azure Blob Storage.
        /// </value>
        [JsonProperty("connectionString")]
        public StringExpression ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the name of the container where entities will be stored.
        /// </summary>
        /// <value>
        /// The name of the container where entities will be stored.
        /// </value>
        [JsonProperty("containerName")]
        public StringExpression ContainerName { get; set; }

        /// <summary>
        /// Register services with the application's service collection.
        /// </summary>
        /// <param name="services">The application's collection of registered services.</param>
        /// <param name="configuration">Application configuration.</param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            services.AddSingleton<IStorage>(new AzureBlobStorage(
                dataConnectionstring: this.ConnectionString?.GetConfigurationValue(configuration),
                containerName: this.ContainerName?.GetConfigurationValue(configuration)));
        }
    }
}
