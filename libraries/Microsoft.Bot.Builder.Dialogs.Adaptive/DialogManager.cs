// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Memory;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Class which runs the dialog system.
    /// </summary>
    public class DialogManager
    {
        private const string LASTACCESS = "_lastAccess";
        private string rootDialogId;
        private string dialogStateProperty;

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogManager"/> class.
        /// </summary>
        /// <param name="rootDialog">rootdialog to use.</param>
        /// <param name="dialogStateProperty">alternate name for the dialogState property. (Default is "DialogState").</param>
        public DialogManager(Dialog rootDialog = null, string dialogStateProperty = null)
        {
            if (rootDialog != null)
            {
                this.RootDialog = rootDialog;
            }

            this.dialogStateProperty = dialogStateProperty ?? "DialogState";
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
        /// Gets turnState to use when turn context happens.
        /// </summary>
        /// <value>
        /// TurnState.
        /// </value>
        public TurnContextStateCollection TurnState { get; private set; } = new TurnContextStateCollection();

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
                if (this.rootDialogId != null)
                {
                    return this.Dialogs.Find(this.rootDialogId);
                }

                return null;
            }

            set
            {
                this.Dialogs = new DialogSet();
                if (value != null)
                {
                    this.rootDialogId = value.Id;
                    this.Dialogs.TelemetryClient = value.TelemetryClient;
                    this.Dialogs.Add(value);
                }
                else
                {
                    this.rootDialogId = null;
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
        /// <param name="cancellationToken">cancelation token.</param>
        /// <returns>result of the running the logic against the activity.</returns>
        public async Task<DialogManagerResult> OnTurnAsync(ITurnContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            var botStateSet = new BotStateSet();

            // preload turnstate with DM turnstate
            foreach (var pair in this.TurnState)
            {
                context.TurnState.Set(pair.Key, pair.Value);
            }

            if (this.ConversationState == null)
            {
                this.ConversationState = context.TurnState.Get<ConversationState>() ?? throw new ArgumentNullException(nameof(this.ConversationState));
            }
            else
            {
                context.TurnState.Set(this.ConversationState);
            }

            botStateSet.Add(this.ConversationState);

            if (this.UserState == null)
            {
                this.UserState = context.TurnState.Get<UserState>();
            }

            if (this.UserState != null)
            {
                botStateSet.Add(this.UserState);
            }

            // create property accessors
            var lastAccessProperty = ConversationState.CreateProperty<DateTime>(LASTACCESS);
            var lastAccess = await lastAccessProperty.GetAsync(context, () => DateTime.UtcNow, cancellationToken: cancellationToken).ConfigureAwait(false);

            // Check for expired conversation
            var now = DateTime.UtcNow;
            if (this.ExpireAfter.HasValue && (DateTime.UtcNow - lastAccess) >= TimeSpan.FromMilliseconds((double)this.ExpireAfter))
            {
                // Clear conversation state
                await ConversationState.ClearStateAsync(context, cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            lastAccess = DateTime.UtcNow;
            await lastAccessProperty.SetAsync(context, lastAccess, cancellationToken: cancellationToken).ConfigureAwait(false);

            // get dialog stack 
            var dialogsProperty = ConversationState.CreateProperty<DialogState>(this.dialogStateProperty);
            DialogState dialogState = await dialogsProperty.GetAsync(context, () => new DialogState(), cancellationToken: cancellationToken).ConfigureAwait(false);

            // Create DialogContext
            var dc = new DialogContext(this.Dialogs, context, dialogState);

            // get the dialogstatemanager configuration
            var dialogStateManager = new DialogStateManager(dc, this.StateConfiguration);
            await dialogStateManager.LoadAllScopesAsync(cancellationToken).ConfigureAwait(false);
            dc.Context.TurnState.Add(dialogStateManager);

            DialogTurnResult turnResult = null;

            // Loop as long as we are getting valid OnError handled we should continue executing the actions for the turn.
            //
            // NOTE: We loop around this block because each pass through we either complete the turn and break out of the loop
            // or we have had an exception AND there was an OnError action which captured the error.  We need to continue the 
            // turn based on the actions the OnError handler introduced.  
            while (true)
            {
                try
                {
                    if (dc.ActiveDialog == null)
                    {
                        // start root dialog
                        turnResult = await dc.BeginDialogAsync(this.rootDialogId, cancellationToken: cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        // Continue execution
                        // - This will apply any queued up interruptions and execute the current/next step(s).
                        turnResult = await dc.ContinueDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

                        if (turnResult.Status == DialogTurnStatus.Empty)
                        {
                            // restart root dialog
                            turnResult = await dc.BeginDialogAsync(this.rootDialogId, cancellationToken: cancellationToken).ConfigureAwait(false);
                        }
                    }

                    // turn successfully completed, break the loop
                    break;
                }
                catch (Exception err)
                {
                    // fire error event, bubbling from the leaf.
                    var handled = await dc.EmitEventAsync(DialogEvents.Error, err, bubble: true, fromLeaf: true, cancellationToken: cancellationToken).ConfigureAwait(false);

                    if (!handled)
                    {
                        // error was NOT handled, throw the exception and end the turn. (This will trigger the Adapter.OnError handler and end the entire dialog stack)
                        throw;
                    }
                }
            }

            // save all state scopes to their respective botState locations.
            await dialogStateManager.SaveAllChangesAsync(cancellationToken).ConfigureAwait(false);

            // save botstate changes
            await botStateSet.SaveAllChangesAsync(dc.Context, false, cancellationToken).ConfigureAwait(false);

            // send trace of memory
            var snapshot = dc.State.GetMemorySnapshot();
            var traceActivity = (Activity)Activity.CreateTraceActivity("BotState", "https://www.botframework.com/schemas/botState", snapshot, "Bot State");
            await dc.Context.SendActivityAsync(traceActivity).ConfigureAwait(false);

            return new DialogManagerResult() { TurnResult = turnResult };
        }
    }
}
