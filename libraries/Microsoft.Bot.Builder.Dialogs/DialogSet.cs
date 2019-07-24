// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// A collection of <see cref="Dialog"/> objects that can all call each other.
    /// </summary>
    public class DialogSet
    {
        private readonly IStatePropertyAccessor<DialogState> _dialogState;
        private readonly IDictionary<string, Dialog> _dialogs = new Dictionary<string, Dialog>();
        private IBotTelemetryClient _telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogSet"/> class.
        /// </summary>
        /// <param name="dialogState">The state property accessor with which to manage the stack for
        /// this dialog set.</param>
        /// <remarks>To start and control the dialogs in this dialog set, create a <see cref="DialogContext"/>
        /// and use its methods to start, continue, or end dialogs. To create a dialog context,
        /// call <see cref="CreateContextAsync(ITurnContext, CancellationToken)"/>.
        /// </remarks>
        public DialogSet(IStatePropertyAccessor<DialogState> dialogState)
        {
            _dialogState = dialogState ?? throw new ArgumentNullException($"missing {nameof(dialogState)}");
            _telemetryClient = NullBotTelemetryClient.Instance;
        }

        internal DialogSet()
        {
            // TODO: This is only used by ComponentDialog and future release
            // will refactor to use IStatePropertyAccessor from context
            _dialogState = null;
            _telemetryClient = NullBotTelemetryClient.Instance;
        }

        /// <summary>
        /// Gets or sets the <see cref="IBotTelemetryClient"/> to use for logging.
        /// </summary>
        /// <value>The <see cref="IBotTelemetryClient"/> to use for logging.</value>
        /// <remarks>When this property is set, it sets the <see cref="Dialog.TelemetryClient"/> of each
        /// dialogs in the set to the new value.</remarks>
        public IBotTelemetryClient TelemetryClient
        {
            get
            {
                return _telemetryClient;
            }

            set
            {
                _telemetryClient = value ?? NullBotTelemetryClient.Instance;
                foreach (var dialog in _dialogs.Values)
                {
                    dialog.TelemetryClient = _telemetryClient;
                }
            }
        }

        /// <summary>
        /// Adds a new dialog to the set and returns the updated set.
        /// </summary>
        /// <param name="dialog">The dialog to add.</param>
        /// <returns>The DialogSet for fluent calls to Add().</returns>
        /// <remarks>Adding a new dialog will inherit the <see cref="IBotTelemetryClient"/> of the DialogSet.</remarks>
        public DialogSet Add(Dialog dialog)
        {
            if (dialog == null)
            {
                throw new ArgumentNullException(nameof(dialog));
            }

            if (_dialogs.ContainsKey(dialog.Id))
            {
                throw new ArgumentException($"DialogSet.Add(): A dialog with an id of '{dialog.Id}' already added.");
            }

            dialog.TelemetryClient = _telemetryClient;
            _dialogs[dialog.Id] = dialog;

            return this;
        }

        public async Task<DialogContext> CreateContextAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            BotAssert.ContextNotNull(turnContext);

            // ToDo: Component Dialog doesn't call this code path. This needs to be cleaned up in 4.1.
            if (_dialogState == null)
            {
                // Note: This shouldn't ever trigger, as the _dialogState is set in the constructor and validated there.
                throw new InvalidOperationException($"DialogSet.CreateContextAsync(): DialogSet created with a null IStatePropertyAccessor.");
            }

            // Load/initialize dialog state
            var state = await _dialogState.GetAsync(turnContext, () => { return new DialogState(); }, cancellationToken).ConfigureAwait(false);

            // Create and return context
            return new DialogContext(this, turnContext, state);
        }

        /// <summary>
        /// Finds a dialog that was previously added to the set using <see cref="Add(Dialog)"/>.
        /// </summary>
        /// <param name="dialogId">ID of the dialog/prompt to look up.</param>
        /// <returns>The dialog if found, otherwise null.</returns>
        public Dialog Find(string dialogId)
        {
            if (string.IsNullOrWhiteSpace(dialogId))
            {
                throw new ArgumentNullException(nameof(dialogId));
            }

            if (_dialogs.TryGetValue(dialogId, out var result))
            {
                return result;
            }

            return null;
        }
    }
}
