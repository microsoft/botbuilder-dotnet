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
    /// <see cref="CosmosDbPartitionedStorage"/> with the application's service collection.
    /// </summary>
    [JsonObject]
    public class CosmosDbPartitionedStorageProvider : IStorageProvider
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.CosmosDbPartitionedStorage";

        /// <summary>
        /// Gets or sets the authentication key for Cosmos DB.
        /// </summary>
        /// <value>
        /// The authentication key for Cosmos DB.
        /// </value>
        [JsonProperty("authenticationKey")]
        public StringExpression AuthenticationKey { get; set; }

        /// <summary>
        /// Gets or sets whether or not to run in Compatibility Mode.
        /// </summary>
        /// <remarks>
        /// Early versions of CosmosDb had a key length limit of 255.  Keys longer than this were
        /// truncated in <see cref="CosmosDbKeyEscape"/>.  This remains the default behavior, but
        /// can be overridden by setting CompatibilityMode to false.  This setting will also allow
        /// for using older collections where no PartitionKey was specified.
        ///
        /// Currently, max key length for Cosmos DB is 1023:
        /// https://docs.microsoft.com/en-us/azure/cosmos-db/concepts-limits#per-item-limits
        /// The default for backwards compatibility is 255 <see cref="CosmosDbKeyEscape.MaxKeyLength"/>.
        /// 
        /// N.B.: CompatibilityMode cannot be 'true' if KeySuffix is used.
        /// </remarks>
        /// <value>
        /// Indicates whether or not to run in Compatibility Mode.
        /// </value>
        [JsonProperty("compatibilityMode")]
        public BoolExpression CompatibilityMode { get; set; }

        /// <summary>
        /// Gets or sets the container identifier.
        /// </summary>
        /// <value>
        /// The container identifier.
        /// </value>
        [JsonProperty("containerId")]
        public StringExpression ContainerId { get; set; }

        /// <summary>
        /// Gets or sets the container throughput to set when creating the container. Defaults to 400.
        /// </summary>
        /// <value>
        /// The container throughput to set when creating the container. Defaults to 400.
        /// </value>
        [JsonProperty("containerThroughput")]
        public IntExpression ContainerThroughput { get; set; }

        /// <summary>
        /// Gets or sets the database ID of the Cosmos DB instance.
        /// </summary>
        /// <value>
        /// The database ID of the Cosmos DB instance.
        /// </value>
        [JsonProperty("databaseId")]
        public StringExpression DatabaseId { get; set; }

        /// <summary>
        /// Gets or sets the Cosmos DB endpoint.
        /// </summary>
        /// <value>
        /// The Cosmos DB endpoint.
        /// </value>
        [JsonProperty("endpoint")]
        public StringExpression Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the suffix to be added to every key. <see cref="CosmosDbKeyEscape.EscapeKey(string)"/>.
        /// </summary>
        /// <remarks>
        /// <see cref = "CompatibilityMode" /> must be set to 'false' to use a KeySuffix.
        /// When KeySuffix is used, keys will NOT be truncated but an exception will be thrown if
        /// the key length is longer than allowed by Cosmos DB.
       /// </remarks>
       /// <value>
       /// String containing only valid CosmosDb key characters. (e.g. not: '\\', '?', '/', '#', '*').
       /// </value>
        [JsonProperty("keySuffix")]
        public StringExpression KeySuffix { get; set; }

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

            var options = new CosmosDbPartitionedStorageOptions
            {
                AuthKey = this.AuthenticationKey?.GetConfigurationValue(configuration),
                CompatibilityMode = this.CompatibilityMode?.GetConfigurationValue(configuration) ?? true,
                ContainerId = this.ContainerId?.GetConfigurationValue(configuration),
                ContainerThroughput = this.ContainerThroughput?.GetConfigurationValue(configuration) ?? 400,
                CosmosDbEndpoint = this.Endpoint?.GetConfigurationValue(configuration),
                DatabaseId = this.DatabaseId?.GetConfigurationValue(configuration),
                KeySuffix = this.KeySuffix?.GetConfigurationValue(configuration)
            };

            services.AddSingleton<IStorage>(_ => new CosmosDbPartitionedStorage(options));
        }
    }
}
