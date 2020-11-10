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
    public class BlobStorageProvider : IStorageProvider
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.BlobStorage";

        [JsonProperty("connectionString")]
        public StringExpression ConnectionString { get; set; }

        [JsonProperty("containerName")]
        public StringExpression ContainerName { get; set; }

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

            // TODO #40: Change to Microsoft.Bot.Builder.Azure.BlobsStorage
            services.AddSingleton<IStorage>(new AzureBlobStorage(
                dataConnectionstring: this.ConnectionString?.GetConfigurationValue(configuration),
                containerName: this.ContainerName?.GetConfigurationValue(configuration)));
        }
    }
}
