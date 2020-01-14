// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Threading.Tasks;
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
        public async Task Generator_sandwich()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection()
                .UseLuisSettings(sandwichDirectory, "TestBot")
                .Build();
            await TestUtils.RunTestScript(configuration: config);
        }

        [TestMethod]
        public async Task Generator_unittests()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection()
                .UseLuisSettings(unitTestsDirectory, "TestBot")
                .Build();
            await TestUtils.RunTestScript(configuration: config);
        }
    }
}
