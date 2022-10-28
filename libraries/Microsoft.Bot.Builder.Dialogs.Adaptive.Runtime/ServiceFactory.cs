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
                    var storageConfig = configuration.GetSection(nameof(CosmosDbPartitionedStorage));
                    if (!storageConfig.Exists())
                    {
                        throw new ArgumentException($"Missing '{nameof(CosmosDbPartitionedStorage)}' configuration.");
                    }

                    var cosmosDbOptions = storageConfig.Get<CosmosDbPartitionedStorageOptions>();
                    return new CosmosDbPartitionedStorage(cosmosDbOptions);
                }

                case nameof(BlobsStorage):
                {
                    // Blob
                    var storageConfig = configuration.GetSection(nameof(BlobsStorage));
                    if (!storageConfig.Exists())
                    {
                        throw new ArgumentException($"Missing '{nameof(BlobsStorage)}' configuration.");
                    }

                    var blobOptions = storageConfig.Get<BlobsStorageSettings>();
                    return new BlobsStorage(blobOptions.ConnectionString, blobOptions.ContainerName);
                }

                default:
                {
                    if (string.IsNullOrEmpty(runtimeSettings?.Storage) 
                        || runtimeSettings.Storage.Equals("Memory", StringComparison.Ordinal))
                    {
                        return new MemoryStorage();
                    }

                    throw new ArgumentException($"Invalid '{ConfigurationConstants.RuntimeSettingsKey}.storage' value.");
                }
            }
        }
    }
}
