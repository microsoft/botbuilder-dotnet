// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers.Tests
{
    [TestClass]
    public class MultiLanguageRecognizerTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task MultiLanguageRecognizerTest_EnUsFallback()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task MultiLanguageRecognizerTest_EnUsFallback_ActivityLocaleCasing()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task MultiLanguageRecognizerTest_EnGbFallback()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task MultiLanguageRecognizerTest_EnFallback()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task MultiLanguageRecognizerTest_DefaultFallback()
        {
            await TestUtils.RunTestScript();
        }
    }
}
