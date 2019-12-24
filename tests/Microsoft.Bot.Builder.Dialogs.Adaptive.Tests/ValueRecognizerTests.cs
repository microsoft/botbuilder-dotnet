// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers.Tests
{
    [TestClass]
    public class ValueRecognizerTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task ValueRecognizerTests_WithIntent()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task ValueRecognizerTests_WithNoIntent()
        {
            await TestUtils.RunTestScript();
        }
    }
}
