// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Memory;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Class which runs the dialog system.
    /// </summary>
    public class DialogManager
    {
        private const string LastAccess = "_lastAccess";
        private string _rootDialogId;
        private readonly string _dialogStateProperty;

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogManager"/> class.
        /// </summary>
        /// <param name="rootDialog">Root dialog to use.</param>
        /// <param name="dialogStateProperty">alternate name for the dialogState property. (Default is "DialogState").</param>
        public DialogManager(Dialog rootDialog = null, string dialogStateProperty = null)
        {
            if (rootDialog != null)
            {
                RootDialog = rootDialog;
            }

            _dialogStateProperty = dialogStateProperty ?? "DialogState";
        }

        /// <summary>
        /// Gets or sets the ConversationState.
        /// </summary>
        /// <value>
        /// The ConversationState.
        /// </value>
        public ConversationState ConversationState { get; set; }

        /// <summary>
        /// Gets or sets the UserState.
        /// </summary>
        /// <value>
        /// The UserState.
        /// </value>
        public UserState UserState { get; set; }

        /// <summary>
        /// Gets InitialTurnState collection to copy into the TurnState on every turn.
        /// </summary>
        /// <value>
        /// TurnState.
        /// </value>
        public TurnContextStateCollection InitialTurnState { get; } = new TurnContextStateCollection();

        /// <summary>
        /// Gets or sets root dialog to use to start conversation.
        /// </summary>
        /// <value>
        /// Root dialog to use to start conversation.
        /// </value>
        public Dialog RootDialog
        {
            get
            {
                if (_rootDialogId != null)
                {
                    return Dialogs.Find(_rootDialogId);
                }

                return null;
            }

            set
            {
                Dialogs = new DialogSet();
                if (value != null)
                {
                    _rootDialogId = value.Id;
                    Dialogs.TelemetryClient = value.TelemetryClient;
                    Dialogs.Add(value);
                    RegisterContainerDialogs(RootDialog, registerRoot: false);
                }
                else
                {
                    _rootDialogId = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets global dialogs that you want to have be callable.
        /// </summary>
        /// <value>Dialogs set.</value>
        [JsonIgnore]
        public DialogSet Dialogs { get; set; } = new DialogSet();

        /// <summary>
        /// Gets or sets the DialogStateManagerConfiguration.
        /// </summary>
        /// <value>
        /// The DialogStateManagerConfiguration.
        /// </value>
        public DialogStateManagerConfiguration StateConfiguration { get; set; }

        /// <summary>
        /// Gets or sets (optional) number of milliseconds to expire the bot's state after.
        /// </summary>
        /// <value>
        /// Number of milliseconds.
        /// </value>
        public int? ExpireAfter { get; set; }

        /// <summary>
        /// Runs dialog system in the context of an ITurnContext.
        /// </summary>
        /// <param name="context">turn context.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>result of the running the logic against the activity.</returns>
        public async Task<DialogManagerResult> OnTurnAsync(ITurnContext context, CancellationToken cancellationToken = default)
        {
            var botStateSet = new BotStateSet();

            // Preload TurnState with DM TurnState.
            foreach (var pair in InitialTurnState)
            {
                context.TurnState.Set(pair.Key, pair.Value);
            }

            // register DialogManager with TurnState.
            context.TurnState.Set(this);

            if (ConversationState == null)
            {
                ConversationState = context.TurnState.Get<ConversationState>() ?? throw new InvalidOperationException($"Unable to get an instance of {nameof(ConversationState)} from turnContext.");
            }
            else
            {
                context.TurnState.Set(ConversationState);
            }

            botStateSet.Add(ConversationState);

            if (UserState == null)
            {
                UserState = context.TurnState.Get<UserState>();
            }
            else
            {
                context.TurnState.Set(UserState);
            }

            if (UserState != null)
            {
                botStateSet.Add(UserState);
            }

            // create property accessors
            var lastAccessProperty = ConversationState.CreateProperty<DateTime>(LastAccess);
            var lastAccess = await lastAccessProperty.GetAsync(context, () => DateTime.UtcNow, cancellationToken).ConfigureAwait(false);

            // Check for expired conversation
            if (ExpireAfter.HasValue && (DateTime.UtcNow - lastAccess) >= TimeSpan.FromMilliseconds((double)ExpireAfter))
            {
                // Clear conversation state
                await ConversationState.ClearStateAsync(context, cancellationToken).ConfigureAwait(false);
            }

            lastAccess = DateTime.UtcNow;
            await lastAccessProperty.SetAsync(context, lastAccess, cancellationToken).ConfigureAwait(false);

            // get dialog stack 
            var dialogsProperty = ConversationState.CreateProperty<DialogState>(_dialogStateProperty);
            var dialogState = await dialogsProperty.GetAsync(context, () => new DialogState(), cancellationToken).ConfigureAwait(false);

            // Create DialogContext
            var dc = new DialogContext(Dialogs, context, dialogState);

            // Call the common dialog "continue/begin" execution pattern shared with the classic RunAsync extension method
            var turnResult = await DialogExtensions.InternalRunAsync(context, _rootDialogId, dc, StateConfiguration, cancellationToken).ConfigureAwait(false);

            // save BotState changes
            await botStateSet.SaveAllChangesAsync(dc.Context, false, cancellationToken).ConfigureAwait(false);

            return new DialogManagerResult { TurnResult = turnResult };
        }

        // Recursively walk up the DC stack to find the active DC.
        private static DialogContext GetActiveDialogContext(DialogContext dialogContext)
        {
            var child = dialogContext.Child;
            if (child == null)
            {
                return dialogContext;
            }

            return GetActiveDialogContext(child);
        }

        /// <summary>
        /// Recursively traverses the <see cref="Dialog"/> tree and registers instances of <see cref="DialogContainer"/>
        /// in the <see cref="DialogSet"/> for this <see cref="DialogManager"/> instance.
        /// </summary>
        /// <param name="dialog">Root of the <see cref="Dialog"/> subtree to iterate and register containers from.</param>
        /// <param name="registerRoot">Whether to register the root of the subtree. </param>
        private void RegisterContainerDialogs(Dialog dialog, bool registerRoot = true)
        {
            if (dialog is DialogContainer container)
            {
                if (registerRoot)
                {
                    Dialogs.Add(container);
                }

                foreach (var inner in container.Dialogs.GetDialogs())
                {
                    // Only continue recursive registration if we have not seen and registered 
                    // the current dialog.
                    if (!Dialogs.GetDialogs().Any(d => d.Id.Equals(inner.Id, StringComparison.Ordinal)))
                    {
                        RegisterContainerDialogs(inner);
                    }
                }
            }
        }
    }
}
