// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma warning disable SA1201 // Elements should appear in the correct order

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class TestScriptTests
    {
        public static IEnumerable<object[]> AssertReplyScripts => TestUtils.GetTestScripts(@"Tests\TestAssertReply");
        
        public static IEnumerable<object[]> AssertReplyOneOfScripts => TestUtils.GetTestScripts(@"Tests\TestAssertReplyOneOf");
        
        public static IEnumerable<object[]> UserActionScripts => TestUtils.GetTestScripts(@"Tests\TestUser");
        
        public TestContext TestContext { get; set; }

        [DataTestMethod]
        [DynamicData(nameof(AssertReplyScripts))]
        public async Task TestScript_AssertReply(string resourceId)
        {
            await TestUtils.RunTestScript(resourceId);
        }

        [DataTestMethod]
        [DynamicData(nameof(AssertReplyOneOfScripts))]
        public async Task TestScript_AssertReplyOne(string resourceId)
        {
            await TestUtils.RunTestScript(resourceId);
        }

        [DataTestMethod]
        [DynamicData(nameof(UserActionScripts))]
        public async Task TestScript_UserAction(string resourceId)
        {
            await TestUtils.RunTestScript(resourceId);
        }

        //[TestMethod]
        //public async Task TestDialog()
        //{
        //    await TestUtils.RunTestScript("Action_EditActionReplaceSequence.test.dialog");
        //}
    }
}
