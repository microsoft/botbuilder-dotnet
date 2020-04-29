// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.Luis.Testing;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class GeneratorTests
    {
        private readonly string sandwichDirectory = PathUtils.NormalizePath(@"..\..\..\..\..\tests\Microsoft.Bot.Builder.Dialogs.Adaptive.Tests\Tests\GeneratorTests\sandwich\");
        private readonly string unitTestsDirectory = PathUtils.NormalizePath(@"..\..\..\..\..\tests\Microsoft.Bot.Builder.Dialogs.Adaptive.Tests\Tests\GeneratorTests\unittests\");

        public static ResourceExplorer ResourceExplorer { get; set; }

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            ResourceExplorer = new ResourceExplorer()
                .AddFolder(Path.Combine(TestUtils.GetProjectPath(), "Tests", nameof(GeneratorTests)), monitorChanges: false)
                .RegisterType(LuisAdaptiveRecognizer.Kind, typeof(MockLuisRecognizer), new MockLuisLoader());
        }

        [TestMethod]
        public async Task Generator_sandwich()
        {
            var config = new ConfigurationBuilder()
                .UseMockLuisSettings(sandwichDirectory, "TestBot")
                .Build();
            HostContext.Current.Set<IConfiguration>(config);

            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task Generator_unittests()
        {
            var config = new ConfigurationBuilder()
                .UseMockLuisSettings(unitTestsDirectory, "TestBot")
                .Build();
            HostContext.Current.Set<IConfiguration>(config);

            await TestUtils.RunTestScript(ResourceExplorer);
        }
    }
}
