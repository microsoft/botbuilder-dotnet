// // Copyright (c) Microsoft Corporation. All rights reserved.
// // Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.BotBuilderSamples.Tests.Utils.XUnit;
using Xunit.Abstractions;

namespace Microsoft.BotBuilderSamples.Tests.Utils
{
    /// <summary>
    /// A Bot to be used for testing dialogs in isolation.
    /// </summary>
    public class DialogsTestBot
    {
        private readonly BotCallbackHandler _callback;
        private readonly TestAdapter _testAdapter;

        public DialogsTestBot(Dialog targetDialog, ITestOutputHelper outputHelper = null, object initialDialogOptions = null)
        {
            var convoState = new ConversationState(new MemoryStorage());
            _testAdapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));
            if (outputHelper != null)
            {
                _testAdapter.Use(new XUnitOutputMiddleware(outputHelper));
            }

            var dialogState = convoState.CreateProperty<DialogState>("DialogState");

            _callback = async (turnContext, cancellationToken) =>
            {
                var state = await dialogState.GetAsync(turnContext, () => new DialogState(), cancellationToken);
                var dialogs = new DialogSet(dialogState);

                dialogs.Add(targetDialog);

                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                switch (results.Status)
                {
                    case DialogTurnStatus.Empty:
                        await dc.BeginDialogAsync(targetDialog.Id, initialDialogOptions, cancellationToken);
                        break;
                    case DialogTurnStatus.Complete:
                    {
                        // TODO: Dialog has ended, figure out a way of asserting that this is the case.
                        break;
                    }
                }
            };
        }

        public async Task<T> SendAsync<T>(string text, CancellationToken cancellationToken = default)
        {
            var task = _testAdapter.SendTextToBotAsync(text, _callback, cancellationToken);
            task.Wait(cancellationToken);
            return await GetNextReplyAsync<T>();
        }

        public Task<T> GetNextReplyAsync<T>()
        {
            return Task.FromResult((T)_testAdapter.GetNextReply());
        }
    }
}
