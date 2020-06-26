// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Schema;
using Xunit;
using dbg = System.Diagnostics;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Teams.Tests
{
    public class ConditionalTests
    {
        public static ResourceExplorer ResourceExplorer { get; set; }

        public static void ClassInitialize()
        {
            ResourceExplorer = new ResourceExplorer()
                .AddFolder(Path.Combine(TestUtils.GetProjectPath(), "Tests", nameof(ConditionalTests)), monitorChanges: false);
        }
        
        [Fact]
        public async Task ConditionalsTests_OnTeamActivityTypes()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }
    }
}
