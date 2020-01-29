// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma warning disable SA1118 // Parameter should not span multiple lines

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class ActionScopeTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task ActionScope_Goto()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task ActionScope_Goto_Parent()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task ActionScope_Goto_OnIntent()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task ActionScope_Goto_Nowhere()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task ActionScope_Break()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task ActionScope_Continue()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task ActionScope_Goto_Switch()
        {
            await TestUtils.RunTestScript();
        }
    }
}
