// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// A related set of dialogs that can all call each other.
    /// </summary>
    public class DialogSet
    {
        private readonly IStatePropertyAccessor<DialogState> _dialogState;
        private readonly IDictionary<string, IDialog> _dialogs = new Dictionary<string, IDialog>();

        private IBotTelemetryClient _telemetryClient;

        public DialogSet(IStatePropertyAccessor<DialogState> dialogState)
        {
            _dialogState = dialogState ?? throw new ArgumentNullException(nameof(dialogState));
            _telemetryClient = NullBotTelemetryClient.Instance;
        }

        public DialogSet()
        {
            _dialogState = null;
            _telemetryClient = NullBotTelemetryClient.Instance;
        }

        /// <summary>
        /// Gets or sets the <see cref="IBotTelemetryClient"/> to use.
        /// When setting this property, all of the contained dialogs' TelemetryClient properties are also set.
        /// </summary>
        /// <value>The <see cref="IBotTelemetryClient"/> to use when logging.</value>
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
        /// Adds a new dialog to the set and returns the added dialog.
        /// If the Dialog.Id being added already exists in the set, the dialogs id will be updated to 
        /// include a suffix which makes it unique.So adding 2 dialogs named "duplicate" to the set
        /// would result in the first one having an id of "duplicate" and the second one having an id
        /// of "duplicate2".
        /// </summary>
        /// <param name="dialog">The dialog to add.</param>
        /// <returns>The DialogSet for fluent calls to Add().</returns>
        /// <remarks>Adding a new dialog will inherit the <see cref="IBotTelemetryClient"/> of the DialogSet.</remarks>
        public DialogSet Add(IDialog dialog)
        {
            if (dialog == null)
            {
                throw new ArgumentNullException(nameof(dialog));
            }

            if (_dialogs.ContainsKey(dialog.Id))
            {
                var nextSuffix = 2;

                while (true)
                {
                    var suffixId = dialog.Id + nextSuffix.ToString();

                    if (!_dialogs.ContainsKey(suffixId))
                    {
                        dialog.Id = suffixId;
                        break;
                    }
                    else
                    {
                        nextSuffix++;
                    }
                }
            }

            dialog.TelemetryClient = _telemetryClient;
            _dialogs[dialog.Id] = dialog;

            // Automatically add any dependencies the dialog might have
            if (dialog is IDialogDependencies dialogWithDependencies)
            {
                dialogWithDependencies.ListDependencies()?.ForEach(d => Add(d));
            }

            return this;
        }

        public Task<DialogContext> CreateContextAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            return CreateContextAsync(turnContext, null, null, cancellationToken);
        }

        public async Task<DialogContext> CreateContextAsync(ITurnContext turnContext, Dictionary<string, object> conversationState, Dictionary<string, object> userState, CancellationToken cancellationToken = default(CancellationToken))
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
            return new DialogContext(this, turnContext, state, conversationState, userState);
        }

        /// <summary>
        /// Finds a dialog that was previously added to the set using <see cref="Add(IDialog)"/>.
        /// </summary>
        /// <param name="dialogId">ID of the dialog/prompt to look up.</param>
        /// <returns>The dialog if found, otherwise null.</returns>
        public IDialog Find(string dialogId)
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

        public IEnumerable<IDialog> GetDialogs()
        {
            return _dialogs.Values;
        }
    }
}
