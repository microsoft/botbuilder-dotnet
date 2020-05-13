// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma warning disable SA1118 // Parameter should not span multiple lines

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class DebugComposer
    {
        public TestContext TestContext { get; set; }

        // This test can be used to debug a composer bot, simple point botPath => composer bot
        // and add debug.test.dialog script to that folder
        // {
        //  "$schema": "../../../tests.schema",
        //  "$kind": "Microsoft.Test.Script",
        //  "dialog": "bot.root.dialog",
        //  "script": [...] 
        // }
        [Ignore]
        [TestMethod]
        public async Task DebugComposerBot()
        {
            // luis.settings.{environment}.{region}.json
            var environment = Environment.UserName;
            var region = "westus";
            var botPath = Path.Combine(TestUtils.GetProjectPath(), "Tests");
            var testScript = "debug.test.dialog";
            var locale = "en-US";

            var resourceExplorer = new ResourceExplorer()
                .AddFolder(botPath, monitorChanges: false);

            // add luis settings if there are luis assets
            var resource = resourceExplorer.GetResource($"luis.settings.{environment}.{region}.json") as FileResource;
            var builder = new ConfigurationBuilder().AddInMemoryCollection();
            if (resource != null)
            {
                builder.AddJsonFile(resource.FullName);
            }

            var script = resourceExplorer.LoadType<TestScript>(testScript);
            script.Locale = locale;
            script.Configuration = builder.Build();
            await script.ExecuteAsync(resourceExplorer: resourceExplorer).ConfigureAwait(false);
        }
    }
}
