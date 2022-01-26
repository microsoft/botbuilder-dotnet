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

        private static readonly string EmulatorPath = Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\Azure Cosmos DB Emulator\CosmosDB.Emulator.exe");

        private static readonly Lazy<bool> HasEmulator = new Lazy<bool>(() =>
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AGENT_NAME")))
            {
                return false;
            }

            if (File.Exists(EmulatorPath))
            {
                var p = new Process
                {
                    StartInfo =
                    {
                        UseShellExecute = true,
                        FileName = EmulatorPath,
                        Arguments = "/GetStatus",
                    },
                };
                p.Start();
                p.WaitForExit();

                return p.ExitCode == 2;
            }

            return false;
        });

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
