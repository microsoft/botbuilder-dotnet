// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;

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
        /// <param name="channelId">
        /// The channelId (see <see cref="Channels"/>) to be used for the test.
        /// Use <see cref="Channels.Emulator"/> or <see cref="Channels.Test"/> if you are uncertain of the channel you are targeting.
        /// Otherwise, it is recommended that you use the id for the channel(s) your bot will be using.
        /// Consider writing a test case for each channel.
        /// </param>
        /// <param name="targetDialog">The dialog to be tested. This will be the root dialog for the test client.</param>
        /// <param name="initialDialogOptions">(Optional) additional argument(s) to pass to the dialog being started.</param>
        /// <param name="middlewares">(Optional) A list of middlewares to be added to the test adapter.</param>
        /// <param name="callback">(Optional) The bot turn processing logic for the test. If this value is not provided, the test client will create a default <see cref="BotCallbackHandler"/>.</param>
        public DialogTestClient(string channelId, Dialog targetDialog, object initialDialogOptions = null, IEnumerable<IMiddleware> middlewares = null, BotCallbackHandler callback = null)
        {
            var convoState = new ConversationState(new MemoryStorage());
            _testAdapter = new TestAdapter(channelId)
                .Use(new AutoSaveStateMiddleware(convoState));

            AddUserMiddlewares(middlewares);

            var dialogState = convoState.CreateProperty<DialogState>("DialogState");

            _callback = callback ?? GetDefaultCallback(targetDialog, initialDialogOptions, dialogState);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogTestClient"/> class.
        /// </summary>
        /// <param name="testAdapter">The <see cref="TestAdapter"/> to use.</param>
        /// <param name="targetDialog">The dialog to be tested. This will be the root dialog for the test client.</param>
        /// <param name="initialDialogOptions">(Optional) additional argument(s) to pass to the dialog being started.</param>
        /// <param name="middlewares">(Optional) A list of middlewares to be added to the test adapter.</param>
        /// <param name="callback">(Optional) The bot turn processing logic for the test. If this value is not provided, the test client will create a default <see cref="BotCallbackHandler"/>.</param>
        public DialogTestClient(TestAdapter testAdapter, Dialog targetDialog, object initialDialogOptions = null, IEnumerable<IMiddleware> middlewares = null, BotCallbackHandler callback = null)
        {
            var convoState = new ConversationState(new MemoryStorage());
            _testAdapter = testAdapter.Use(new AutoSaveStateMiddleware(convoState));

            AddUserMiddlewares(middlewares);

            var dialogState = convoState.CreateProperty<DialogState>("DialogState");

            _callback = callback ?? GetDefaultCallback(targetDialog, initialDialogOptions, dialogState);
        }

        /// <summary>
        /// Gets the latest <see cref="DialogTurnResult"/> for the dialog being tested.
        /// </summary>
        /// <value>A <see cref="DialogTurnResult"/> instance with the result of the last turn.</value>
        public virtual DialogTurnResult DialogTurnResult { get; private set; }

        /// <summary>
        /// Sends an <see cref="Activity"/> to the target dialog.
        /// </summary>
        /// <param name="activity">The activity to send.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        /// <typeparam name="T">An <see cref="IActivity"/> derived type.</typeparam>
        public virtual async Task<T> SendActivityAsync<T>(Activity activity, CancellationToken cancellationToken = default)
            where T : IActivity
        {
            await _testAdapter.ProcessActivityAsync(activity, _callback, cancellationToken).ConfigureAwait(false);
            return GetNextReply<T>();
        }

        /// <summary>
        /// Sends a message activity to the target dialog.
        /// </summary>
        /// <param name="text">The text of the message to send.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        /// <typeparam name="T">An <see cref="IActivity"/> derived type.</typeparam>
        public virtual async Task<T> SendActivityAsync<T>(string text, CancellationToken cancellationToken = default)
            where T : IActivity
        {
            await _testAdapter.SendTextToBotAsync(text, _callback, cancellationToken).ConfigureAwait(false);
            return GetNextReply<T>();
        }

        /// <summary>
        /// Gets the next bot response.
        /// </summary>
        /// <returns>The next activity in the queue; or null, if the queue is empty.</returns>
        /// <typeparam name="T">An <see cref="IActivity"/> derived type.</typeparam>
        public virtual T GetNextReply<T>()
            where T : IActivity
        {
            return (T)_testAdapter.GetNextReply();
        }

        private BotCallbackHandler GetDefaultCallback(Dialog targetDialog, object initialDialogOptions, IStatePropertyAccessor<DialogState> dialogState) =>
            async (turnContext, cancellationToken) =>
            {
                // Ensure dialog state is created and pass it to DialogSet.
                await dialogState.GetAsync(turnContext, () => new DialogState(), cancellationToken).ConfigureAwait(false);
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

        private void AddUserMiddlewares(IEnumerable<IMiddleware> middlewares)
        {
            if (middlewares != null)
            {
                foreach (var middleware in middlewares)
                {
                    _testAdapter.Use(middleware);
                }
            }
        }
    }
}
