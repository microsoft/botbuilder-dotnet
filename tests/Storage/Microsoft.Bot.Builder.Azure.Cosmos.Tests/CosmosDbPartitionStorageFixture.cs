// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Xunit;

namespace Microsoft.Bot.Builder.Azure.Cosmos.Tests
{
    public class CosmosDbPartitionStorageFixture : IAsyncLifetime
    {
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
                    CosmosDbTestConstants.CosmosServiceEndpoint,
                    CosmosDbTestConstants.CosmosAuthKey,
                    new CosmosClientOptions());

                await client.CreateDatabaseIfNotExistsAsync(CosmosDbTestConstants.CosmosDatabaseName);
            }
        }

        public async Task DisposeAsync()
        {
            if (HasEmulator.Value)
            {
                var client = new CosmosClient(
                    CosmosDbTestConstants.CosmosServiceEndpoint,
                    CosmosDbTestConstants.CosmosAuthKey,
                    new CosmosClientOptions());
                try
                {
                    await client.GetDatabase(CosmosDbTestConstants.CosmosDatabaseName).DeleteAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error cleaning up resources: {0}", ex.ToString());
                }
            }
        }
    }
}
