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
            await TestUtils.RunTestScript("Action_BeginDialog.test.dialog");
        }

        [TestMethod]
        public async Task Action_ChoiceInput()
        {
            await TestUtils.RunTestScript("Action_ChoiceInput.test.dialog");
        }

        [TestMethod]
        public async Task Action_ChoiceInput_WithLocale()
        {
            await TestUtils.RunTestScript("Action_ChoiceInput_WithLocale.test.dialog");
        }

        [TestMethod]
        public async Task Action_ChoicesInMemory()
        {
            await TestUtils.RunTestScript("Action_ChoicesInMemory.test.dialog");
        }

        [TestMethod]
        public async Task Action_ChoiceStringInMemory()
        {
            await TestUtils.RunTestScript("Action_ChoiceStringInMemory.test.dialog");
        }

        [TestMethod]
        public async Task Action_ConfirmInput()
        {
            await TestUtils.RunTestScript("Action_ConfirmInput.test.dialog");
        }

        [TestMethod]
        public async Task Action_DatetimeInput()
        {
            await TestUtils.RunTestScript("Action_DatetimeInput.test.dialog");
        }

        [TestMethod]
        public async Task Action_DoActions()
        {
            await TestUtils.RunTestScript("Action_DoActions.test.dialog");
        }

        [TestMethod]
        public async Task Action_EditActionReplaceSequence()
        {
            await TestUtils.RunTestScript("Action_EditActionReplaceSequence.test.dialog");
        }

        [TestMethod]
        public async Task Action_EmitEvent()
        {
            await TestUtils.RunTestScript("Action_EmitEvent.test.dialog");
        }

        [TestMethod]
        public async Task Action_EndDialog()
        {
            await TestUtils.RunTestScript("Action_EndDialog.test.dialog");
        }

        [TestMethod]
        public async Task Action_Foreach()
        {
            await TestUtils.RunTestScript("Action_Foreach.test.dialog");
        }

        [TestMethod]
        public async Task Action_ForeachPage()
        {
            await TestUtils.RunTestScript("Action_ForeachPage.test.dialog");
        }

        [TestMethod]
        public async Task Action_IfCondition()
        {
            await TestUtils.RunTestScript("Action_IfCondition.test.dialog");
        }

        [TestMethod]
        public async Task Action_NumberInput()
        {
            await TestUtils.RunTestScript("Action_NumberInput.test.dialog");
        }

        [TestMethod]
        public async Task Action_NumberInputWithDefaultValue()
        {
            await TestUtils.RunTestScript("Action_NumberInputWithDefaultValue.test.dialog");
        }

        [TestMethod]
        public async Task Action_NumberInputWithVAlueExpression()
        {
            await TestUtils.RunTestScript("Action_NumberInputWithVAlueExpression.test.dialog");
        }

        [TestMethod]
        public async Task Action_RepeatDialog()
        {
            await TestUtils.RunTestScript("Action_RepeatDialog.test.dialog");
        }

        [TestMethod]
        public async Task Action_ReplaceDialog()
        {
            await TestUtils.RunTestScript("Action_ReplaceDialog.test.dialog");
        }

        [TestMethod]
        public async Task Action_Switch()
        {
            await TestUtils.RunTestScript("Action_Switch.test.dialog");
        }

        [TestMethod]
        public async Task Action_Switch_Bool()
        {
            await TestUtils.RunTestScript("Action_Switch_Bool.test.dialog");
        }

        [TestMethod]
        public async Task Action_Switch_Default()
        {
            await TestUtils.RunTestScript("Action_Switch_Default.test.dialog");
        }

        [TestMethod]
        public async Task Action_Switch_Number()
        {
            await TestUtils.RunTestScript("Action_Switch_Number.test.dialog");
        }

        [TestMethod]
        public async Task Action_TextInput()
        {
            await TestUtils.RunTestScript("Action_TextInput.test.dialog");
        }

        [TestMethod]
        public async Task Action_TextInputWithInvalidPrompt()
        {
            await TestUtils.RunTestScript("Action_TextInputWithInvalidPrompt.test.dialog");
        }

        [TestMethod]
        public async Task Action_TraceActivity()
        {
            await TestUtils.RunTestScript("Action_TraceActivity.test.dialog");
        }

        [TestMethod]
        public async Task Action_WaitForInput()
        {
            await TestUtils.RunTestScript("Action_WaitForInput.test.dialog");
        }

        [TestMethod]
        public async Task InputDialog_ActivityProcessed()
        {
            await TestUtils.RunTestScript("InputDialog_ActivityProcessed.test.dialog");
        }
    }
}
