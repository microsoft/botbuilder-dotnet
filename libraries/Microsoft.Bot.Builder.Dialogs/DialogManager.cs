// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using static Microsoft.Bot.Builder.Dialogs.DialogContext;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Class which runs the dialog system.
    /// </summary>
    public class DialogManager
    {
        private DialogSet dialogSet;
        private string rootDialogId;

        public DialogManager(IDialog rootDialog = null)
        {
            this.dialogSet = new DialogSet();

            if (rootDialog != null)
            {
                this.RootDialog = rootDialog;
            }
        }

        /// <summary>
        /// Root dialog to use to start conversation.
        /// </summary>
        public IDialog RootDialog
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
                this.dialogSet = new DialogSet();
                this.dialogSet.Add(value);
            }
        }

        /// <summary>
        /// Run a dialog purely by processing an activity and getting the result. 
        /// </summary>
        /// <remarks>
        /// NOTE: does not support any activity semantic other then SendActivity
        /// </remarks>
        /// <param name="activity">activity to process</param>
        /// <param name="state">state to use</param>
        /// <returns>result of the running the logic against the activity.</returns>
        public async Task<DialogManagerResult> RunAsync(Activity activity, StoredBotState state = null)
        {
            // Initialize context object
            var adapter = new DialogManagerAdapter();
            var context = new TurnContext(adapter, activity);
            var result = await this.OnTurnAsync(context, state).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Runs dialog system in the context of an ITurnContext.
        /// </summary>
        /// <param name="context">turn context.</param>
        /// <param name="storedState">stored state.</param>
        /// <param name="cancellationToken">cancelation token.</param>
        /// <returns>result of the running the logic against the activity.</returns>
        public async Task<DialogManagerResult> OnTurnAsync(ITurnContext context, StoredBotState storedState = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var saveState = false;
            var keys = ComputeKeys(context);
            var storage = context.TurnState.Get<IStorage>();

            if (storedState == null)
            {
                storedState = await LoadBotState(storage, keys).ConfigureAwait(false);
                saveState = true;
            }

            var dc = new DialogContext(
                dialogSet,
                context,
                new DialogState()
                {
                    ConversationState = storedState.ConversationState,
                    UserState = storedState.UserState,
                    DialogStack = storedState.DialogStack,
                },
                conversationState: storedState.ConversationState,
                userState: storedState.UserState);

            // Dispatch ActivityReceived event
            // This will queue up any interruptions
            await dc.EmitEventAsync(DialogEvents.ActivityReceived, value: context.Activity, bubble: true, fromLeaf: true, cancellationToken: cancellationToken).ConfigureAwait(false);

            // Continue execution
            // This will apply any queued up interruptions and execute the current / next step(s)
            var result = await dc.ContinueDialogAsync(cancellationToken).ConfigureAwait(false);

            if (result.Status == DialogTurnStatus.Empty)
            {
                result = await dc.BeginDialogAsync(this.rootDialogId, cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            if (saveState)
            {
                await SaveBotState(storage, storedState, keys).ConfigureAwait(false);
                return new DialogManagerResult()
                {
                    TurnResult = result,
                };
            }
            else
            {
                return new DialogManagerResult()
                {
                    TurnResult = result,
                    NewState = storedState,
                };
            }
        }
        private static async Task<StoredBotState> LoadBotState(IStorage storage, BotStateStorageKeys keys)
        {
            var data = await storage.ReadAsync(new[] { keys.UserState, keys.ConversationState, keys.DialogState }).ConfigureAwait(false);

            return new StoredBotState()
            {
                UserState = data.ContainsKey(keys.UserState) ? data[keys.UserState] as IDictionary<string, object> : new Dictionary<string, object>(),
                ConversationState = data.ContainsKey(keys.ConversationState) ? data[keys.ConversationState] as IDictionary<string, object> : new Dictionary<string, object>(),
                DialogStack = data.ContainsKey(keys.DialogState) ? data[keys.DialogState] as IList<DialogInstance> : new List<DialogInstance>(),
            };
        }

        private static async Task SaveBotState(IStorage storage, StoredBotState newState, BotStateStorageKeys keys)
        {
            await storage.WriteAsync(new Dictionary<string, object>()
            {
                { keys.UserState, newState.UserState},
                { keys.ConversationState, newState.ConversationState },
                { keys.DialogState, newState.DialogStack },
            }).ConfigureAwait(false);
        }

        private static BotStateStorageKeys ComputeKeys(ITurnContext context)
        {
            // Get channel, user and conversation ids
            var activity = context.Activity;
            var channelId = activity.ChannelId;
            var userId = activity.From?.Id;
            var conversationId = activity.Conversation?.Id;

            // Patch user id if needed
            if (activity.Type == ActivityTypes.ConversationUpdate)
            {
                var members = activity.MembersAdded ?? activity.MembersRemoved ?? new List<ChannelAccount>();
                var nonRecipients = members.Where(m => m.Id != activity.Recipient.Id);
                var found = userId != null ? nonRecipients.FirstOrDefault(r => r.Id == userId) : null;

                if (found == null && members.Count > 0)
                {
                    userId = nonRecipients.FirstOrDefault()?.Id ?? userId;
                }
            }

            // Verify ids were found
            if (userId == null)
            {
                throw new Exception("PlanningDialog: unable to load the bots state.The users ID couldn't be found.");
            }

            if (conversationId == null)
            {
                throw new Exception("PlanningDialog: unable to load the bots state. The conversations ID couldn't be found.");
            }

            // Return storage keys
            return new BotStateStorageKeys()
            {
                UserState = $"{channelId}/users/{userId}",
                ConversationState = $"{channelId}/conversations/{conversationId}",
                DialogState = $"{channelId}/dialog/{conversationId}",
            };
        }
    }
}
