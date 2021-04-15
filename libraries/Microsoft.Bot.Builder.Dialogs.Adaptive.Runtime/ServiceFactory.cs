// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Azure.Blobs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime
{
    internal static class ServiceFactory
    {
        public static IStorage Storage(IServiceProvider serviceProvider)
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var runtimeSettings = configuration.GetSection(ConfigurationConstants.RuntimeSettingsKey).Get<RuntimeSettings>();

            switch (runtimeSettings?.Storage)
            {
                case nameof(CosmosDbPartitionedStorage):
                {
                    // Cosmosdb
                    var cosmosDbOptions = configuration?.GetSection(nameof(CosmosDbPartitionedStorage)).Get<CosmosDbPartitionedStorageOptions>();
                    return new CosmosDbPartitionedStorage(cosmosDbOptions);
                }

                case nameof(BlobsStorage):
                {
                    // Blob
                    var blobOptions = configuration?.GetSection(nameof(BlobsStorage)).Get<BlobsStorageSettings>();
                    return new BlobsStorage(blobOptions?.ConnectionString, blobOptions?.ContainerName);
                }

                default:
                    return new MemoryStorage();
            }
        }
    }
}
