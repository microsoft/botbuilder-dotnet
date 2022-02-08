using System;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    public sealed class IgnoreOnNoEmulatorFact : FactAttribute
    {
        public IgnoreOnNoEmulatorFact()
        {
            Skip = @"
This test has been disabled as part of the issue (https://github.com/microsoft/botbuilder-dotnet/issues/6023).

Reasons:
  - CosmosDB tests that are using the emulator will be moved to the BotFramework-FunctionalTests repo, issue (https://github.com/microsoft/BotFramework-FunctionalTests/issues/552).
  - The process that corroborates the CosmosDB Emulator is up and running was being triggered multiple times.
    - This process was being executed by having the Test Explorer open and without any CosmosDB test running.
    - The test was silently skipped if the user didn't start the emulator first.
  - This test was already being skipped from the build server, because the emulator wasn't running, which was disabled in the PR (https://github.com/microsoft/botbuilder-dotnet/pull/3815/files#diff-f471e8f21408a57242b52ff8234a71e650ca6ed7c93721f1e6dc53c66989d6f8R10).
            ".Trim();
        }
    }
}
