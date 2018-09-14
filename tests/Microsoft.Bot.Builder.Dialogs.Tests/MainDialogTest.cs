// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Tests;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{

    public class TestMainDialog : MainDialog
    {
        public TestMainDialog(IStatePropertyAccessor<DialogState> state)
            : base(state, nameof(MainDialog))
        {
            this.DialogStateProperty = state;

            AddDialog(WaterfallTests.Create_Waterfall3());
            AddDialog(WaterfallTests.Create_Waterfall4());
            AddDialog(WaterfallTests.Create_Waterfall5());
        }
    }


    [TestClass]
    [TestCategory("MainDialog")]
    public class MainDialogTest
    {
        [TestMethod]
        public void DialogSet_ConstructorValid()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogStateProperty = convoState.CreateProperty<DialogState>("dialogstate");
            var mainDialog = new MainDialog(dialogStateProperty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DialogSet_ConstructorNullProperty()
        {
            var mainDialog = new MainDialog(null);
        }

        [TestMethod]
        public async Task DialogSet_CreateContextAsync()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogStateProperty = convoState.CreateProperty<DialogState>("dialogstate");
            var mainDialog = new TestMainDialog(dialogStateProperty);
            var context = TestUtilities.CreateEmptyContext();
            var dc = await mainDialog.RunAsync(context);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DialogSet_NullCreateContextAsync()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogStateProperty = convoState.CreateProperty<DialogState>("dialogstate");
            var mainDialog = new TestMainDialog(dialogStateProperty);
            var dc = await mainDialog.RunAsync(null);
        }

        [TestMethod]
        public async Task DialogSet_FullDialogs()
        {
            var convoState = new ConversationState(new MemoryStorage());

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var mainDialog = new TestMainDialog(convoState.CreateProperty<DialogState>("dialogState") );
            
            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await mainDialog.RunAsync(turnContext).ConfigureAwait(false);
            })
            .Send("hello")
                .AssertReply("step1")
                .AssertReply("step1.1")
            .Send("hello")
                .AssertReply("step1.2")
            .Send("hello")
                .AssertReply("step2")
                .AssertReply("step2.1")
            .Send("hello")
                .AssertReply("step2.2")
            .StartTestAsync();
        }
    }
}
