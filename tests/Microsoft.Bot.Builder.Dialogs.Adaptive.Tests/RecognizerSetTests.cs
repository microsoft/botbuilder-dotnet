// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers.Tests
{
    [TestClass]
    public class RecognizerSetTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task RecognizerSetTests_DoubleDefer()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task RecognizerSetTests_DoubleIntent()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task RecognizerSetTests_NoneWithIntent()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task RecognizerSetTests_AllNone()
        {
            await TestUtils.RunTestScript();
        }
    }
}
