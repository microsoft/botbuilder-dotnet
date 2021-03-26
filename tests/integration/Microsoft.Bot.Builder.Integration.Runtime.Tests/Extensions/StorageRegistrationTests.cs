// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Azure.Blobs;
using Microsoft.Bot.Builder.Integration.Runtime.Extensions;
using Microsoft.Bot.Builder.Integration.Runtime.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests.Extensions
{
    public class StorageRegistrationTests
    {
        public static IEnumerable<object[]> StorageRegistrationTestData()
        {
            // params: Dictionary<string, string> config, ResourcesSettings settings, Type registeredType
            yield return new object[]
            {
                new Dictionary<string, string>(),
                null,
                typeof(MemoryStorage)
            };
            yield return new object[]
            {
                new Dictionary<string, string>(),
                new RuntimeSettings(),
                typeof(MemoryStorage)
            };
            yield return new object[]
            {
                new Dictionary<string, string>
                {
                    { $"{typeof(CosmosDbPartitionedStorage).Name}:authKey", "authKey" },
                    { $"{typeof(CosmosDbPartitionedStorage).Name}:containerId", "containerId" },
                    { $"{typeof(CosmosDbPartitionedStorage).Name}:cosmosDbEndpoint", "cosmosDbEndpoint" },
                    { $"{typeof(CosmosDbPartitionedStorage).Name}:databaseId", "databaseId" },
                },
                new RuntimeSettings() { Storage = typeof(CosmosDbPartitionedStorage).Name },
                typeof(CosmosDbPartitionedStorage)
            };
            yield return new object[]
            {
                new Dictionary<string, string>
                {
                    { $"{typeof(BlobsStorage).Name}:connectionString", "UseDevelopmentStorage=true" },
                    { $"{typeof(BlobsStorage).Name}:containerName", "containerName" },
                },
                new RuntimeSettings() { Storage = typeof(BlobsStorage).Name },
                typeof(BlobsStorage)
            };
        }

        public static IEnumerable<object[]> StorageRegistrationTestErrorData()
        {
            // params: Dictionary<string, string> config, ResourcesSettings settings, Type exceptionType
            yield return new object[]
            {
                new Dictionary<string, string>
                {
                    { $"{typeof(CosmosDbPartitionedStorage).Name}:authKey", string.Empty },
                    { $"{typeof(CosmosDbPartitionedStorage).Name}:containerId", "containerId" },
                    { $"{typeof(CosmosDbPartitionedStorage).Name}:cosmosDbEndpoint", "cosmosDbEndpoint" },
                    { $"{typeof(CosmosDbPartitionedStorage).Name}:databaseId", "databaseId" },
                },
                new RuntimeSettings() { Storage = typeof(CosmosDbPartitionedStorage).Name },
                typeof(ArgumentException)
            };
            yield return new object[]
            {
                new Dictionary<string, string>
                {
                    { $"{typeof(BlobsStorage).Name}:connectionString", "badformat" },
                    { $"{typeof(BlobsStorage).Name}:containerName", "containerName" },
                },
                new RuntimeSettings() { Storage = typeof(BlobsStorage).Name },
                typeof(FormatException)
            };
            yield return new object[]
            {
                new Dictionary<string, string>
                {
                    { $"{typeof(BlobsStorage).Name}:containerName", "containerName" },
                },
                new RuntimeSettings() { Storage = typeof(BlobsStorage).Name },
                typeof(ArgumentNullException)
            };
        }

        //[Theory]
        //[MemberData(nameof(StorageRegistrationTestData))]
        public void AddBotRuntimeStorage(Dictionary<string, string> config, Type registeredType)
        {
            // Setup
            IServiceCollection services = new ServiceCollection();
            IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(config).Build();

            services.AddSingleton(configuration);

            // Test
            services.AddBotRuntimeStorage();

            // Assert
            var provider = services.BuildServiceProvider();

            Assert.IsType(registeredType, provider.GetService<IStorage>());
        }

        //[Theory]
        //[MemberData(nameof(StorageRegistrationTestErrorData))]
        public void AddBotRuntimeStorage_ErrorCase(Dictionary<string, string> config, object settings, Type expectedException)
        {
            // Setup
            IServiceCollection services = new ServiceCollection();
            IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(config).Build();
            var resourcesSettings = settings as RuntimeSettings;

            // Test
            services.AddBotRuntimeStorage();

            // Assert
            var provider = services.BuildServiceProvider();
            Assert.Throws(expectedException, () => provider.GetService<IStorage>());
        }
    }
}
