// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class MiscTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task IfCondition_EndDialog()
        {
            await TestUtils.RunTestScript("IfCondition_EndDialog.test.dialog");
        }

        [TestMethod]
        public async Task Rule_Reprompt()
        {
            await TestUtils.RunTestScript("Rule_Reprompt.test.dialog");
        }
    }
}
