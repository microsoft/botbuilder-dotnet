// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Memory;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing
{
    /// <summary>
    /// Delegate for inspecting or modifying dialog state.
    /// </summary>
    /// <param name="dc">Dialog context.</param>
    public delegate void DialogContextInspector(DialogContext dc);

    /// <summary>
    /// Class for inspecting current dialog context.
    /// </summary>
    internal class DialogInspector
    {
        private string _rootDialogId;
        private readonly string _dialogStateProperty;

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogInspector"/> class.
        /// </summary>
        /// <param name="rootDialog">Root dialog to use.</param>
        /// <param name="resourceExplorer">Resource explorer for expression access to .lg templates.</param>
        /// <param name="dialogStateProperty">Alternate name for the dialogState property. (Default is "DialogState").</param>
        public DialogInspector(Dialog rootDialog = null, ResourceExplorer resourceExplorer = null, string dialogStateProperty = null)
        {
            if (resourceExplorer != null)
            {
                // Add language generator for function access
                InitialTurnState.Add(new LanguageGeneratorManager(resourceExplorer));
            }

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
        /// Inspects a dialogs memory.
        /// </summary>
        /// <param name="context">turn context.</param>
        /// <param name="inspector">Inspector for analyzing/modifying dialog context.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>result of the running the logic against the activity.</returns>
        public async Task InspectAsync(ITurnContext context, DialogContextInspector inspector, CancellationToken cancellationToken = default)
        {
            // This class just lets you load & save memory in parallel
            var botStateSet = new BotStateSet();

            // Some of the memory scopes expect to find things like storage in the turn state
            foreach (var pair in InitialTurnState)
            {
                context.TurnState.Set(pair.Key, pair.Value);
            }

            // register DialogManager with TurnState.
            context.TurnState.Set(this);

            // At a minimum you need ConversationState. UserState is optional
            if (ConversationState == null)
            {
                ConversationState = context.TurnState.Get<ConversationState>() ?? throw new NullReferenceException(nameof(ConversationState));
            }
            else
            {
                context.TurnState.Set(ConversationState);
            }

            // Add conversation state & user state to our parallel class
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

            // get dialog stack 
            var dialogsProperty = ConversationState.CreateProperty<DialogState>(_dialogStateProperty);
            var dialogState = await dialogsProperty.GetAsync(context, () => new DialogState(), cancellationToken).ConfigureAwait(false);

            // Create DialogContext
            var dc = new DialogContext(Dialogs, context, dialogState);

            // promote initial TurnState into dc.services for contextual services
            foreach (var service in dc.Services)
            {
                dc.Services[service.Key] = service.Value;
            }

            // map TurnState into root dialog context.services
            foreach (var service in context.TurnState)
            {
                dc.Services[service.Key] = service.Value;
            }

            // get the DialogStateManager configuration
            // - this configures all of the memory scopes and makes sure all of their memory has been loaded.
            var dialogStateManager = new DialogStateManager(dc, StateConfiguration);
            await dialogStateManager.LoadAllScopesAsync(cancellationToken).ConfigureAwait(false);
            dc.Context.TurnState.Add(dialogStateManager);

            // Find the DC for the active dialog
            var activeDc = GetActiveDialogContext(dc);

            inspector(activeDc);  
            
            // save all state scopes to their respective botState locations.
            await dialogStateManager.SaveAllChangesAsync(cancellationToken).ConfigureAwait(false);
            await botStateSet.SaveAllChangesAsync(dc.Context, false, cancellationToken).ConfigureAwait(false);
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
    }
}
