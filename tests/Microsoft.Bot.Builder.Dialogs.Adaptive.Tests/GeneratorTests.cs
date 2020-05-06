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
    public class GeneratorTests
    {
        private readonly string sandwichDirectory = PathUtils.NormalizePath(@"..\..\..\..\..\tests\Microsoft.Bot.Builder.Dialogs.Adaptive.Tests\Tests\GeneratorTests\sandwich\");
        private readonly string unitTestsDirectory = PathUtils.NormalizePath(@"..\..\..\..\..\tests\Microsoft.Bot.Builder.Dialogs.Adaptive.Tests\Tests\GeneratorTests\unittests\");

        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task Generator_sandwich()
        {
            var config = new ConfigurationBuilder()
                .UseMockLuisSettings(sandwichDirectory, "TestBot")
                .Build();
            
            var resourceExplorer = new ResourceExplorer()
                .AddFolder(Path.Combine(TestUtils.GetProjectPath(), "Tests", nameof(GeneratorTests)), monitorChanges: false)
                .RegisterType(LuisAdaptiveRecognizer.Kind, typeof(MockLuisRecognizer), new MockLuisLoader(config));

            await TestUtils.RunTestScript(resourceExplorer, configuration: config);
        }

        [TestMethod]
        public async Task Generator_unittests()
        {
            var config = new ConfigurationBuilder()
                .UseMockLuisSettings(unitTestsDirectory, "TestBot")
                .Build();

            var resourceExplorer = new ResourceExplorer()
                .AddFolder(Path.Combine(TestUtils.GetProjectPath(), "Tests", nameof(GeneratorTests)), monitorChanges: false)
                .RegisterType(LuisAdaptiveRecognizer.Kind, typeof(MockLuisRecognizer), new MockLuisLoader(config));

            await TestUtils.RunTestScript(resourceExplorer, configuration: config);
        }
    }
}
