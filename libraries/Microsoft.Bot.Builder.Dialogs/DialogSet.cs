﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// A collection of <see cref="Dialog"/> objects that can all call each other.
    /// </summary>
    public class DialogSet
    {
        private readonly IDictionary<string, Dialog> _dialogs;
        private readonly IStatePropertyAccessor<DialogState> _dialogState;
        private IBotTelemetryClient _telemetryClient;
        private string _version;

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogSet"/> class.
        /// </summary>
        /// <param name="dialogState">The state property accessor with which to manage the stack for
        /// this dialog set.</param>
        /// <param name="stringComparer">The <see cref="StringComparer"/> to use for the Dictionary within the DialogSet.</param>
        /// <remarks>To start and control the dialogs in this dialog set, create a <see cref="DialogContext"/>
        /// and use its methods to start, continue, or end dialogs. To create a dialog context,
        /// call <see cref="CreateContextAsync(ITurnContext, CancellationToken)"/>.
        /// </remarks>
        public DialogSet(IStatePropertyAccessor<DialogState> dialogState, StringComparer stringComparer = default(StringComparer))
        {
            _dialogs = new Dictionary<string, Dialog>(stringComparer ?? StringComparer.OrdinalIgnoreCase);
            _dialogState = dialogState ?? throw new ArgumentNullException(nameof(dialogState));
            _telemetryClient = NullBotTelemetryClient.Instance;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogSet"/> class with null <see cref="DialogState"/>.
        /// </summary>
        public DialogSet()
        {
            _dialogState = null;
            _dialogs = new Dictionary<string, Dialog>(StringComparer.OrdinalIgnoreCase);
            _telemetryClient = NullBotTelemetryClient.Instance;
        }

        /// <summary>
        /// Gets or sets the <see cref="IBotTelemetryClient"/> to use for logging.
        /// </summary>
        /// <value>The <see cref="IBotTelemetryClient"/> to use for logging.</value>
        /// <remarks>When this property is set, it sets the <see cref="Dialog.TelemetryClient"/> of each
        /// dialog in the set to the new value.</remarks>
        [JsonIgnore]
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
        /// Gets a unique string which represents the combined versions of all dialogs in this this dialogset.  
        /// </summary>
        /// <returns>Version will change when any of the child dialogs version changes.</returns>
        public virtual string GetVersion()
        {
            if (_version == null)
            {
                var sb = new StringBuilder();
                foreach (var dialog in _dialogs)
                {
                    var v = _dialogs[dialog.Key].GetVersion();
                    if (v != null)
                    {
                        sb.Append(v);
                    }
                }

                _version = StringUtils.Hash(sb.ToString());
            }

            return _version;
        }

        /// <summary>
        /// Adds a new dialog to the set and returns the set to allow fluent chaining.
        /// If the Dialog.Id being added already exists in the set, the dialogs id will be updated to 
        /// include a suffix which makes it unique. So adding 2 dialogs named "duplicate" to the set
        /// would result in the first one having an id of "duplicate" and the second one having an id
        /// of "duplicate2".
        /// </summary>
        /// <param name="dialog">The dialog to add.</param>
        /// <returns>The dialog set after the operation is complete.</returns>
        /// <remarks>The added dialog's <see cref="Dialog.TelemetryClient"/> is set to the
        /// <see cref="TelemetryClient"/> of the dialog set.</remarks>
        public DialogSet Add(Dialog dialog)
        {
            // Ensure new version hash is computed
            _version = null;

            if (dialog == null)
            {
                throw new ArgumentNullException(nameof(dialog));
            }

            if (_dialogs.ContainsKey(dialog.Id))
            {
                // If we are trying to add the same exact instance, it's not a name collision.
                // No operation required since the instance is already in the dialog set.
                if (_dialogs[dialog.Id] == dialog)
                {
                    return this;
                }

                // If we are adding a new dialog with a conflicting name, add a suffix to avoid
                // dialog name collisions.
                var nextSuffix = 2;

                while (true)
                {
                    var suffixId = dialog.Id + nextSuffix;

                    if (!_dialogs.ContainsKey(suffixId))
                    {
                        dialog.Id = suffixId;
                        break;
                    }

                    nextSuffix++;
                }
            }

            dialog.TelemetryClient = _telemetryClient;
            _dialogs[dialog.Id] = dialog;

            return this;
        }

        /// <summary>
        /// Creates a <see cref="DialogContext"/> which can be used to work with the dialogs in the
        /// <see cref="DialogSet"/>.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>If the task is successful, the result contains the created <see cref="DialogContext"/>.
        /// </remarks>
        public async Task<DialogContext> CreateContextAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            // ToDo: Component Dialog doesn't call this code path. This needs to be cleaned up in 4.1.
            if (_dialogState == null)
            {
                // Note: This shouldn't ever trigger, as the _dialogState is set in the constructor and validated there.
                throw new InvalidOperationException("DialogSet.CreateContextAsync(): DialogSet created with a null IStatePropertyAccessor.");
            }

            // Load/initialize dialog state
            var state = await _dialogState.GetAsync(turnContext, () => { return new DialogState(); }, cancellationToken).ConfigureAwait(false);

            // Create and return context
            return new DialogContext(this, turnContext, state);
        }

        /// <summary>
        /// Searches the current <see cref="DialogSet"/> for a <see cref="Dialog"/> by its ID.
        /// </summary>
        /// <param name="dialogId">ID of the dialog to search for.</param>
        /// <returns>The dialog if found; otherwise <c>null</c>.</returns>
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

        /// <summary>
        /// Gets the Dialogs of the set.
        /// </summary>
        /// <returns>A collection of <see cref="Dialog"/>.</returns>
        public IEnumerable<Dialog> GetDialogs()
        {
            return _dialogs.Values;
        }
    }
}
