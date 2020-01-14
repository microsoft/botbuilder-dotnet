// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.MockLuis;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class LuisRecognizerTests
    {
        private readonly string dynamicListsDirectory = PathUtils.NormalizePath(@"..\..\..\..\..\tests\Microsoft.Bot.Builder.Dialogs.Adaptive.Tests\Tests\LuisRecognizerTests\");

        [TestMethod]
        public async Task DynamicLists()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection()
                .UseLuisSettings(dynamicListsDirectory, "TestBot")
                .Build();
            await TestUtils.RunTestScript(configuration: config);
        }
    }
}
