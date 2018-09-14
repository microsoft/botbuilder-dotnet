// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Tests
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
        public void MainDialog_ConstructorValid()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogStateProperty = convoState.CreateProperty<DialogState>("dialogstate");
            var mainDialog = new MainDialog(dialogStateProperty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MainDialog_ConstructorNullProperty()
        {
            var mainDialog = new MainDialog(null);
        }

        [TestMethod]
        public async Task MainDialog_CreateContextAsync()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogStateProperty = convoState.CreateProperty<DialogState>("dialogstate");
            var mainDialog = new TestMainDialog(dialogStateProperty);
            var context = TestUtilities.CreateEmptyContext();
            var dc = await mainDialog.RunAsync(context);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task MainDialog_NullCreateContextAsync()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogStateProperty = convoState.CreateProperty<DialogState>("dialogstate");
            var mainDialog = new TestMainDialog(dialogStateProperty);
            var dc = await mainDialog.RunAsync(null);
        }

        [TestMethod]
        public async Task MainDialog_DerivedMainDialog()
        {
            var convoState = new ConversationState(new MemoryStorage());

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var mainDialog = new TestMainDialog(convoState.CreateProperty<DialogState>("dialogState"));

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

        [TestMethod]
        public async Task MainDialog_DynamicMainDialog()
        {
            var convoState = new ConversationState(new MemoryStorage());

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var mainDialog = new MainDialog(convoState.CreateProperty<DialogState>("dialogState"));
            mainDialog
                .AddDialog(WaterfallTests.Create_Waterfall3())
                .AddDialog(WaterfallTests.Create_Waterfall4())
                .AddDialog(WaterfallTests.Create_Waterfall5());

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
