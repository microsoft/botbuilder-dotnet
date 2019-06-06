// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Testing.XUnit;
using Xunit.Abstractions;

namespace Microsoft.Bot.Builder.Testing
{
    /// <summary>
    /// A client to for testing dialogs in isolation.
    /// </summary>
    public class DialogTestClient
    {
        private readonly BotCallbackHandler _callback;
        private readonly TestAdapter _testAdapter;

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogTestClient"/> class.
        /// </summary>
        /// <param name="targetDialog">The dialog to be tested. This will be the root dialog for the test client.</param>
        /// <param name="initialDialogOptions">(Optional) additional argument(s) to pass to the dialog being started.</param>
        /// <param name="outputHelper">
        /// An XUnit <see cref="ITestOutputHelper"/> instance.
        /// See <see href="https://xunit.net/docs/capturing-output.html">Capturing Output</see> in the XUnit documentation for additional details.
        /// If this value is set, the test client will output the incoming and outgoing activities to the console window.
        /// </param>
        public DialogTestClient(Dialog targetDialog, object initialDialogOptions = null, ITestOutputHelper outputHelper = null)
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
                var state = await dialogState.GetAsync(turnContext, () => new DialogState(), cancellationToken).ConfigureAwait(false);
                var dialogs = new DialogSet(dialogState);

                dialogs.Add(targetDialog);

                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken).ConfigureAwait(false);

                DialogTurnResult = await dc.ContinueDialogAsync(cancellationToken).ConfigureAwait(false);
                switch (DialogTurnResult.Status)
                {
                    case DialogTurnStatus.Empty:
                        DialogTurnResult = await dc.BeginDialogAsync(targetDialog.Id, initialDialogOptions, cancellationToken).ConfigureAwait(false);
                        break;
                    case DialogTurnStatus.Complete:
                    {
                        // Dialog has ended
                        break;
                    }
                }
            };
        }

        /// <summary>
        /// Gets the latest <see cref="DialogTurnResult"/> for the dialog being tested.
        /// </summary>
        /// <value>A <see cref="DialogTurnResult"/> instance with the result of the last turn.</value>
        public DialogTurnResult DialogTurnResult { get; private set; }

        public async Task<T> SendAsync<T>(string text, CancellationToken cancellationToken = default)
        {
            var task = _testAdapter.SendTextToBotAsync(text, _callback, cancellationToken);
            task.Wait(cancellationToken);
            return await GetNextReplyAsync<T>().ConfigureAwait(false);
        }

        public Task<T> GetNextReplyAsync<T>()
        {
            return Task.FromResult((T)_testAdapter.GetNextReply());
        }
    }
}
