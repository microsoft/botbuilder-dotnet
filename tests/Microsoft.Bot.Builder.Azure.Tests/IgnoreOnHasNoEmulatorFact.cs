using System;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    public sealed class IgnoreOnHasNoEmulatorFact : FactAttribute
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

        public IgnoreOnHasNoEmulatorFact()
        {
            if (!HasEmulator.Value)
            {
                Skip = "This test requires CosmosDB Emulator! go to https://aka.ms/documentdb-emulator-docs to download and install.";
            }
        }
    }
}
