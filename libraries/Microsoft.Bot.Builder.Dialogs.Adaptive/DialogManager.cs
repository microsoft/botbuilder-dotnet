// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Memory;
using Microsoft.Bot.Builder.Dialogs.Memory.Scopes;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Class which runs the dialog system.
    /// </summary>
    public class DialogManager
    {
        private const string DIALOGS = "_dialogs";
        private const string LASTACCESS = "_lastAccess";
        private DialogSet dialogSet;
        private string rootDialogId;

        public DialogManager(Dialog rootDialog = null)
        {
            this.dialogSet = new DialogSet();

            if (rootDialog != null)
            {
                this.RootDialog = rootDialog;
            }
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
                    return this.dialogSet.Find(this.rootDialogId);
                }

                return null;
            }

            set
            {
                this.dialogSet = new DialogSet();
                if (value != null)
                {
                    this.rootDialogId = value.Id;
                    this.dialogSet.Add(value);
                }
                else
                {
                    this.rootDialogId = null;
                }
            }
        }

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
            BotStateSet botStateSet = new BotStateSet();
            ConversationState conversationState = this.ConversationState ?? context.TurnState.Get<ConversationState>() ?? throw new ArgumentNullException($"{nameof(ConversationState)} is not found in the turn context. Have you called adapter.UseState() with a configured ConversationState object?");
            UserState userState = this.UserState ?? context.TurnState.Get<UserState>();
            if (conversationState != null)
            {
                botStateSet.Add(conversationState); 
            }

            if (userState != null)
            {
                botStateSet.Add(userState);
            }

            // create property accessors
            var lastAccessProperty = conversationState.CreateProperty<DateTime>(LASTACCESS);
            var lastAccess = await lastAccessProperty.GetAsync(context, () => DateTime.UtcNow, cancellationToken: cancellationToken).ConfigureAwait(false);

            // Check for expired conversation
            var now = DateTime.UtcNow;
            if (this.ExpireAfter.HasValue && (DateTime.UtcNow - lastAccess) >= TimeSpan.FromMilliseconds((double)this.ExpireAfter))
            {
                // Clear conversation state
                await conversationState.ClearStateAsync(context, cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            lastAccess = DateTime.UtcNow;
            await lastAccessProperty.SetAsync(context, lastAccess, cancellationToken: cancellationToken).ConfigureAwait(false);

            // get dialog stack 
            var dialogsProperty = conversationState.CreateProperty<DialogState>(DIALOGS);
            DialogState dialogState = await dialogsProperty.GetAsync(context, () => new DialogState(), cancellationToken: cancellationToken).ConfigureAwait(false);

            // Create DialogContext
            var dc = new DialogContext(this.dialogSet, context, dialogState);

            // set DSM configuration
            dc.SetStateConfiguration(this.StateConfiguration ?? DialogStateManager.CreateStandardConfiguration(conversationState, userState));

            // load scopes
            await dc.GetState().LoadAllScopesAsync(cancellationToken).ConfigureAwait(false);

            DialogTurnResult turnResult = null;
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

            // save all state scopes to their respective stores.
            await dc.GetState().SaveAllChangesAsync(cancellationToken).ConfigureAwait(false);

            // save botstate changes
            await botStateSet.SaveAllChangesAsync(dc.Context, false, cancellationToken).ConfigureAwait(false);

            // send trace of memory
            var snapshot = dc.GetState().GetMemorySnapshot();
            var traceActivity = (Activity)Activity.CreateTraceActivity("BotState", "https://www.botframework.com/schemas/botState", snapshot, "Bot State");
            await dc.Context.SendActivityAsync(traceActivity).ConfigureAwait(false);

            return new DialogManagerResult() { TurnResult = turnResult };
        }
    }
}
