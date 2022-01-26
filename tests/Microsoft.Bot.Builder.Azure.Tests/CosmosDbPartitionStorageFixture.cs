using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Xunit;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    public class CosmosDbPartitionStorageFixture : IAsyncLifetime
    {
        private const string CosmosServiceEndpoint = "https://localhost:8081";
        private const string CosmosAuthKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private const string CosmosDatabaseName = "test-CosmosDbPartitionStorageTests";

        // This process has been disabled, more information can be found in the tests\Microsoft.Bot.Builder.Azure.Tests\IgnoreOnNoEmulatorFact.cs file.
        private static readonly Lazy<bool> HasEmulator = new Lazy<bool>(() => false);

        public async Task InitializeAsync()
        {
            if (HasEmulator.Value)
            {
                var client = new CosmosClient(
                    CosmosServiceEndpoint,
                    CosmosAuthKey,
                    new CosmosClientOptions());

                await client.CreateDatabaseIfNotExistsAsync(CosmosDatabaseName);
            }
        }

        public async Task DisposeAsync()
        {
            if (HasEmulator.Value)
            {
                var client = new CosmosClient(
                    CosmosServiceEndpoint,
                    CosmosAuthKey,
                    new CosmosClientOptions());
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
}
