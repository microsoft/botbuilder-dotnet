// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers.Tests
{
    [TestClass]
    public class RecognizerTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task RecognizerTest_EnUsFallback()
        {
            await TestUtils.RunTestScript("RecognizerTest_EnUsFallback.test.dialog");
        }

        [TestMethod]
        public async Task RecognizerTest_EnUsFallback_ActivityLocaleCasing()
        {
            await TestUtils.RunTestScript("RecognizerTest_EnUsFallback_ActivityLocaleCasing.test.dialog");
        }

        [TestMethod]
        public async Task RecognizerTest_EnGbFallback()
        {
            await TestUtils.RunTestScript("RecognizerTest_EnGbFallback.test.dialog");
        }

        [TestMethod]
        public async Task RecognizerTest_EnFallback()
        {
            await TestUtils.RunTestScript("RecognizerTest_EnFallback.test.dialog");
        }

        [TestMethod]
        public async Task RecognizerTest_DefaultFallback()
        {
            await TestUtils.RunTestScript("RecognizerTest_DefaultFallback.test.dialog");
        }
    }
}
