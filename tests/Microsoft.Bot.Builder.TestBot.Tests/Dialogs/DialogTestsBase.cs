// // Copyright (c) Microsoft Corporation. All rights reserved.
// // Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.BotBuilderSamples.Tests.Dialogs
{
    public class DialogTestsBase
    {
        /// <summary>
        /// Factory method to create a <see cref="TestFlow"/>.
        /// </summary>
        protected static TestFlow BuildTestFlow(Dialog targetDialog)
        {
            var convoState = new ConversationState(new MemoryStorage());
            var testAdapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));
            var dialogState = convoState.CreateProperty<DialogState>("DialogState");
            var testFlow = new TestFlow(testAdapter, async (turnContext, cancellationToken) =>
            {
                var state = await dialogState.GetAsync(turnContext, () => new DialogState(), cancellationToken);
                var dialogs = new DialogSet(dialogState);

                dialogs.Add(targetDialog);

                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                switch (results.Status)
                {
                    case DialogTurnStatus.Empty:
                        await dc.BeginDialogAsync(targetDialog.Id, null, cancellationToken);
                        break;
                    case DialogTurnStatus.Complete:
                    {
                        // TODO: Dialog has ended, figure out a way of asserting that this is the case.
                        break;
                    }
                }
            });
            return testFlow;
        }
    }
}
