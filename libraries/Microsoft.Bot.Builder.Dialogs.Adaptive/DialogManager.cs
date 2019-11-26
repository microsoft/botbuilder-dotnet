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
                this.rootDialogId = value.Id;
                this.dialogSet.Add(value);
            }
        }

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
            ConversationState conversationState = context.TurnState.Get<ConversationState>() ?? throw new ArgumentNullException($"{nameof(ConversationState)} is not found in the turn context. Have you called adapter.UseState() with a configured ConversationState object?");
            UserState userState = context.TurnState.Get<UserState>() ?? throw new ArgumentNullException($"{nameof(UserState)} is not found in the turn context. Have you called adapter.UseState() with a configured UserState object?"); 

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

            // send trace of memory
            await dc.Context.SendActivityAsync((Activity)Activity.CreateTraceActivity("BotState", "https://www.botframework.com/schemas/botState", dc.GetState().GetMemorySnapshot(), "Bot State")).ConfigureAwait(false);

            return new DialogManagerResult() { TurnResult = turnResult };
        }
    }
}
