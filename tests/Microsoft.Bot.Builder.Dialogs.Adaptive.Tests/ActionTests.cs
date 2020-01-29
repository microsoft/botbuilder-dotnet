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
    public class ActionTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task Action_BeginDialog()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_BeginDialogWithActivity()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_BeginDialogWithoutActivity()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_ChoiceInput()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_ChoiceInput_WithLocale()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_ChoicesInMemory()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_ChoiceStringInMemory()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_ConfirmInput()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_DatetimeInput()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_DeleteActivity()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_DoActions()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_EditActionReplaceSequence()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_EmitEvent()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_EndDialog()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_Foreach()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_ForeachPage()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_ForeachPage_Empty()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_ForeachPage_Partial()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_GetActivityMembers()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_GetConversationMembers()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_IfCondition()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_NumberInput()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_NumberInputWithDefaultValue()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_NumberInputWithVAlueExpression()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_RepeatDialog()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_ReplaceDialog()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_SignOutUser()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_Switch()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_Switch_Bool()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_Switch_Default()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_Switch_Number()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_TextInput()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_TextInputWithInvalidPrompt()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_TextInputWithValueExpression()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_TraceActivity()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_UpdateActivity()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_WaitForInput()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task InputDialog_ActivityProcessed()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_SendActivity()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_SetProperty()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_SetProperties()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_DeleteProperty()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task Action_DeleteProperties()
        {
            await TestUtils.RunTestScript();
        }
    }
}
