// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Microsoft.Bot.Builder.Dialogs.DialogContext;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Class which runs the dialog system.
    /// </summary>
    public class DialogManager
    {
        private const string DIALOGS = "_dialogs";
        private const string LASTACCESS = "_lastAccess";
        private const string ETAG = "eTag";
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
        /// (Optional) number of milliseconds to expire the bots state after.
        /// </summary>
        public int? ExpireAfter { get; set; }

        /// <summary>
        /// (Optional) storage provider that will be used to read and write the bots state..
        /// </summary>
        public IStorage Storage { get; set; }

        /// <summary>
        /// Run a dialog purely by processing an activity and getting the result. 
        /// </summary>
        /// <remarks>
        /// NOTE: does not support any activity semantic other then SendActivity
        /// </remarks>
        /// <param name="activity">activity to process</param>
        /// <param name="state">state to use</param>
        /// <returns>result of the running the logic against the activity.</returns>
        public async Task<DialogManagerResult> RunAsync(Activity activity, PersistedState state = null)
        {
            // Initialize context object
            var adapter = new DialogManagerAdapter();
            var context = new TurnContext(adapter, activity);
            var result = await this.OnTurnAsync(context, state).ConfigureAwait(false);
            result.Activities = adapter.Activities.ToArray();
            return result;
        }

        /// <summary>
        /// Runs dialog system in the context of an ITurnContext.
        /// </summary>
        /// <param name="context">turn context.</param>
        /// <param name="state">stored state.</param>
        /// <param name="cancellationToken">cancelation token.</param>
        /// <returns>result of the running the logic against the activity.</returns>
        public async Task<DialogManagerResult> OnTurnAsync(ITurnContext context, PersistedState state = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var saveState = false;
            var keys = GetKeys(context);
            var storage = context.TurnState.Get<IStorage>();

            if (state == null)
            {
                if (storage == null)
                {
                    throw new Exception("DialogManager: unable to load the bots state.Bot.storage not assigned.");
                }

                state = await LoadState(storage, keys).ConfigureAwait(false);
                saveState = true;
            }

            // Clone state to preserve original state
            var newState = ObjectPath.Clone(state);

            // Check for expired conversation
            var now = DateTime.UtcNow;

            if (this.ExpireAfter.HasValue && newState.ConversationState.ContainsKey(LASTACCESS))
            {
                var lastAccess = DateTime.Parse(newState.ConversationState[LASTACCESS] as string);
                if ((DateTime.UtcNow - lastAccess) >= TimeSpan.FromMilliseconds((double)this.ExpireAfter))
                {
                    // Clear conversation state
                    state.ConversationState = new Dictionary<string, object>();
                    state.ConversationState[ETAG] = newState.ConversationState[ETAG];
                }
            }

            newState.ConversationState[LASTACCESS] = DateTime.UtcNow.ToString("u");

            // Ensure dialog stack populated
            DialogState dialogState;
            if (!newState.ConversationState.ContainsKey(DIALOGS))
            {
                dialogState = new DialogState();
                newState.ConversationState[DIALOGS] = dialogState;
            }
            else
            {
                dialogState = (DialogState)newState.ConversationState[DIALOGS];
            }

            // Create DialogContext
            var dc = new DialogContext(
                this.dialogSet,
                context,
                dialogState,
                conversationState: newState.ConversationState,
                userState: newState.UserState,
                settings: null);

            DialogTurnResult turnResult = null;
            if (dc.ActiveDialog == null)
            {
                // start root dialog
                turnResult = await dc.BeginDialogAsync(this.rootDialogId, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // Dispatch "activityReceived" event
                // - This will queue up any interruptions.
                var handled = await dc.EmitEventAsync(DialogEvents.ActivityReceived, value: dc.Context.Activity, bubble: true, fromLeaf: true, cancellationToken: cancellationToken).ConfigureAwait(false);

                // Continue execution
                // - This will apply any queued up interruptions and execute the current/next step(s).
                turnResult = await dc.ContinueDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

                if (turnResult.Status == DialogTurnStatus.Empty)
                {
                    // start root dialog
                    turnResult = await dc.BeginDialogAsync(this.rootDialogId, cancellationToken: cancellationToken).ConfigureAwait(false);
                }
            }

            // ORPHANED CODE? https://github.com/microsoft/botbuilder-dotnet/issues/2194 
            // Save snapshot of final state for the turn
            // context.TurnState.Set(PersistedStateSnapshotKey, newState);

            // Save state if loaded from storage
            if (saveState)
            {
                await DialogManager.SaveState(storage, keys: keys, newState: newState, oldState: state, eTag: "*").ConfigureAwait(false);
                return new DialogManagerResult() { TurnResult = turnResult };
            }
            else
            {
                return new DialogManagerResult() { TurnResult = turnResult, NewState = newState };
            }
        }

        public static async Task<PersistedState> LoadState(IStorage storage, PersistedStateKeys keys)
        {
            var data = await storage.ReadAsync(keys.ToArray()).ConfigureAwait(false);

            return new PersistedState(keys, data);
        }

        public static async Task SaveState(IStorage storage, PersistedStateKeys keys, PersistedState newState, PersistedState oldState = null, string eTag = null)
        {
            // Check for state changes
            var save = false;
            Dictionary<string, object> changes = new Dictionary<string, object>();
            if (oldState != null)
            {
                if (JsonConvert.SerializeObject(newState.UserState) != JsonConvert.SerializeObject(oldState.UserState))
                {
                    if (eTag != null)
                    {
                        newState.UserState[ETAG] = eTag;
                    }

                    changes[keys.UserState] = newState.UserState;
                    save = true;
                }

                if (JsonConvert.SerializeObject(newState.ConversationState) != JsonConvert.SerializeObject(oldState.ConversationState))
                {
                    if (eTag != null)
                    {
                        newState.ConversationState[ETAG] = eTag;
                    }

                    changes[keys.ConversationState] = newState.ConversationState;
                    save = true;
                }
            }
            else
            {
                if (eTag != null)
                {
                    newState.UserState[ETAG] = eTag;
                    newState.ConversationState[ETAG] = eTag;
                }

                changes[keys.UserState] = newState.UserState;
                changes[keys.ConversationState] = newState.ConversationState;
                save = true;
            }

            // Save changes
            if (save)
            {
                await storage.WriteAsync(changes).ConfigureAwait(false);
            }
        }

        public static PersistedStateKeys GetKeys(ITurnContext context)
        {
            // Get channel, user and conversation ids
            var activity = context.Activity;
            var reference = context.Activity.GetConversationReference();
            if (reference.User == null)
            {
                reference.User = new ChannelAccount();
            }

            if (activity.Type == ActivityTypes.ConversationUpdate)
            {
                var users = (activity.MembersAdded ?? activity.MembersRemoved ?? new List<ChannelAccount>()).Where((u) => u.Id != activity.Recipient.Id).ToList();
                var found = string.IsNullOrEmpty(reference.User?.Id) ? users.Where((u) => u.Id == reference.User.Id).ToList() : new List<ChannelAccount>();

                if (found.Any())
                {
                    reference.User.Id = users[0].Id;
                }
            }

            // Return keys
            return GetKeysForReference(reference);
        }

        public static PersistedStateKeys GetKeysForReference(ConversationReference reference, string @namespace = null)
        {
            // Get channel, user, and conversation ID's
            string channelId = reference.ChannelId;
            string userId = reference.User?.Id;
            string conversationId = reference.Conversation?.Id;

            // Verify ID's found
            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("DialogManager: unable to load/save the bots state. The users ID couldn't be found.");
            }

            if (string.IsNullOrEmpty(conversationId))
            {
                throw new Exception("DialogManager: unable to load / save the bots state.The conversations ID couldn't be found.");
            }

            // Return storage keys
            return new PersistedStateKeys()
            {
                UserState = $"{channelId}/users/{userId}",
                ConversationState = $"{channelId}/conversations/{conversationId}/{@namespace}"
            };
        }
    }
}
