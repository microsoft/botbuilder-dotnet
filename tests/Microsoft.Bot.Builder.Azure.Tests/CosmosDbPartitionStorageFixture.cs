// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Xunit;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    public class CosmosDbPartitionStorageFixture : IAsyncLifetime
    {
        public const string CosmosDatabaseName = "test-CosmosDbPartitionStorageTests";
        public const string CosmosCollectionName = "bot-storage";

        private readonly CosmosDbFixture cosmosDbFixture;
        private CosmosClient client;

        public CosmosDbPartitionStorageFixture(CosmosDbFixture cosmosDbFixture)
        {
            this.cosmosDbFixture = cosmosDbFixture;
        }

        public async Task InitializeAsync()
        {
            if (!cosmosDbFixture.IsEmulatorRunning)
            {
                return;
            }

            client = new CosmosClient(
                CosmosDbFixture.CosmosServiceEndpoint,
                CosmosDbFixture.CosmosAuthKey,
                new CosmosClientOptions());

            await client.CreateDatabaseIfNotExistsAsync(CosmosDatabaseName);
        }

        public async Task DisposeAsync()
        {
            if (!cosmosDbFixture.IsEmulatorRunning)
            {
                return;
            }

            try
            {
                await client.GetDatabase(CosmosDatabaseName).DeleteAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error cleaning up resources: {0}", ex.ToString());
            }
        }
    }
}
