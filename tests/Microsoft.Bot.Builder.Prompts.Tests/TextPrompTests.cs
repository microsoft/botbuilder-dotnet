// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Prompts.Tests
{
    [TestClass]
    [TestCategory("Prompts")]
    public class TextPrompTests
    {
        [TestMethod]
        public async Task FirstTest()
        {
            TextPrompt tp = new TextPrompt();
            Assert.IsNotNull(tp);
        }
    }
}