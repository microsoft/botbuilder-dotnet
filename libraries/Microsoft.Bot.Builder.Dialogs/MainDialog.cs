// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// MainDialog handles the basic structure of running the root of a dialog stack.
    /// </summary>
    public class MainDialog : ComponentDialog
    {
        private DialogSet _mainDialogSet;

        public MainDialog(IStatePropertyAccessor<DialogState> dialogState, string dialogId = nameof(MainDialog))
            : base(dialogId)
        {
            DialogStateProperty = dialogState ?? throw new ArgumentNullException(nameof(dialogState));

            // create dialog set for the outer dialog
            _mainDialogSet = new DialogSet(DialogStateProperty);
            _mainDialogSet.Add(this);
        }

        /// <summary>
        /// Gets or sets the DialogState property Accessor.
        /// </summary>
        /// <value>
        /// A property accessor for the current DialogState.
        /// </value>
        protected IStatePropertyAccessor<DialogState> DialogStateProperty { get; set; }

        /// <summary>
        /// Run the current dialog stack.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <returns>The result of running the current main dialog.</returns>
        public async Task<DialogTurnResult> RunAsync(ITurnContext turnContext)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            // Create a dialog context and try to continue running the current dialog
            var dialogContext = await _mainDialogSet.CreateContextAsync(turnContext).ConfigureAwait(false);

            // continue any running dialog
            var result = await dialogContext.ContinueDialogAsync().ConfigureAwait(false);

            // if there isn't one running, then start main dialog as the root
            if (result.Status == DialogTurnStatus.Empty)
            {
                result = await dialogContext.BeginDialogAsync(this.Id).ConfigureAwait(false);
            }

            return result;
        }
    }
}
