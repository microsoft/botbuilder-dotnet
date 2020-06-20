// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using dbg = System.Diagnostics;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Teams.Tests
{
    [TestClass]
    public class ConditionalTests
    {
        public static ResourceExplorer ResourceExplorer { get; set; }

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            ResourceExplorer = new ResourceExplorer()
                .AddFolder(Path.Combine(TestUtils.GetProjectPath(), "Tests", nameof(ConditionalTests)), monitorChanges: false);
        }
        
        [TestMethod]
        public async Task ConditionalsTests_OnTeamActivityTypes()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }
    }
}
