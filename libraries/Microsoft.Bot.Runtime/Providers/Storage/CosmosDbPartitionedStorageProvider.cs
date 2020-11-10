// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Runtime.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Microsoft.Bot.Runtime.Providers.Storage
{
    [JsonObject]
    public class CosmosDbPartitionedStorageProvider : IStorageProvider
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.CosmosDbPartitionedStorage";

        [JsonProperty("authenticationKey")]
        public StringExpression AuthenticationKey { get; set; }

        [JsonProperty("compatibilityMode")]
        public BoolExpression CompatibilityMode { get; set; }

        [JsonProperty("containerId")]
        public StringExpression ContainerId { get; set; }

        [JsonProperty("containerThroughput")]
        public IntExpression ContainerThroughput { get; set; }

        [JsonProperty("databaseId")]
        public StringExpression DatabaseId { get; set; }

        [JsonProperty("endpoint")]
        public StringExpression Endpoint { get; set; }

        [JsonProperty("keySuffix")]
        public StringExpression KeySuffix { get; set; }

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
