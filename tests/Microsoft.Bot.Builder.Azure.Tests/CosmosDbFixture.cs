// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    public class CosmosDbFixture : IAsyncLifetime
    {
        // Endpoint and Authkey for the CosmosDB Emulator running locally.
        // See https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator?tabs=ssl-netstd21#authenticate-requests for details on the well known key being used.
        public const string CosmosServiceEndpoint = "https://localhost:8081";
        public const string CosmosAuthKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        private static readonly string EmulatorPath = Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\Azure Cosmos DB Emulator\CosmosDB.Emulator.exe");

        public bool IsEmulatorRunning { get; private set; }

        public Task InitializeAsync()
        {
            IsEmulatorRunning = IsEmulatorStarted();

            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public void SkipIfEmulatorIsNotRunning()
        {
            Skip.IfNot(IsEmulatorRunning, "This test requires CosmosDB Emulator to be installed and running! more information can be found in https://aka.ms/documentdb-emulator-docs.");
        }

        protected bool IsEmulatorStarted()
        {
            var agentName = Environment.GetEnvironmentVariable("AGENT_NAME");
            if (!string.IsNullOrEmpty(agentName) || !File.Exists(EmulatorPath))
            {
                return false;
            }

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
    }
}
