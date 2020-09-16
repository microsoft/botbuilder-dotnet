// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.Luis.Testing;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core.Tests;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class AssignEntityTests
    {
        public static ResourceExplorer ResourceExplorer { get; set; }

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            string assignEntityTestsDirectory = PathUtils.NormalizePath(@"..\..\..\..\..\tests\Microsoft.Bot.Builder.Dialogs.Adaptive.Tests\Tests\AssignEntityTests\");

            var config = new ConfigurationBuilder()
           .UseMockLuisSettings(assignEntityTestsDirectory, "TestBot")
           .Build();

            var path = Path.Combine(TestUtils.GetProjectPath(), "Tests", nameof(AssignEntityTests));

            ResourceExplorer = new ResourceExplorer()
                .AddFolder(path, monitorChanges: false)
                .RegisterType(LuisAdaptiveRecognizer.Kind, typeof(MockLuisRecognizer), new MockLuisLoader(config));
        }

        [TestMethod]
        public async Task AddEntity()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task ShowEntity()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task ClearEntity()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }
    }
}
