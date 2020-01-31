// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.MockLuis;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class GeneratorTests
    {
        private readonly string sandwichDirectory = PathUtils.NormalizePath(@"..\..\..\..\..\tests\Microsoft.Bot.Builder.Dialogs.Adaptive.Tests\Tests\GeneratorTests\sandwich\");
        private readonly string unitTestsDirectory = PathUtils.NormalizePath(@"..\..\..\..\..\tests\Microsoft.Bot.Builder.Dialogs.Adaptive.Tests\Tests\GeneratorTests\unittests\");

        [TestMethod]
        public async Task SandwichOrder()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection()
                .UseLuisSettings(sandwichDirectory, "generatorTests")
                .Build();
            await TestUtils.RunTestScript("generator_sandwich.test.dialog", configuration: config);
        }

        [TestMethod]
        public async Task UnitTests()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection()
                .UseLuisSettings(unitTestsDirectory, "generatorTests")
                .Build();
            await TestUtils.RunTestScript("generator_unittests.test.dialog", configuration: config);
        }
    }
}
