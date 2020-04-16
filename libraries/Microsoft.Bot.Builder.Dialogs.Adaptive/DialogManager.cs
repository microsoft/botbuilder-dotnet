// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Memory;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
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
        /// Gets turnState to use when turn context happens.
        /// </summary>
        /// <value>
        /// TurnState.
        /// </value>
        public TurnContextStateCollection TurnState { get; } = new TurnContextStateCollection();

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
            foreach (var pair in TurnState)
            {
                context.TurnState.Set(pair.Key, pair.Value);
            }

            if (ConversationState == null)
            {
                ConversationState = context.TurnState.Get<ConversationState>() ?? throw new ArgumentNullException(nameof(ConversationState));
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

            // get the DialogStateManager configuration
            var dialogStateManager = new DialogStateManager(dc, StateConfiguration);
            await dialogStateManager.LoadAllScopesAsync(cancellationToken).ConfigureAwait(false);
            dc.Context.TurnState.Add(dialogStateManager);

            DialogTurnResult turnResult = null;

            // Loop as long as we are getting valid OnError handled we should continue executing the actions for the turn.
            //
            // NOTE: We loop around this block because each pass through we either complete the turn and break out of the loop
            // or we have had an exception AND there was an OnError action which captured the error.  We need to continue the 
            // turn based on the actions the OnError handler introduced.
            var endOfTurn = false;
            while (!endOfTurn)
            {
                try
                {
                    if (context.TurnState.Get<IIdentity>(BotAdapter.BotIdentityKey) is ClaimsIdentity claimIdentity && SkillValidation.IsSkillClaim(claimIdentity.Claims))
                    {
                        // The bot is running as a skill.
                        turnResult = await HandleSkillOnTurnAsync(dc, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        // The bot is running as root bot.
                        turnResult = await HandleBotOnTurnAsync(dc, cancellationToken).ConfigureAwait(false);
                    }

                    // turn successfully completed, break the loop
                    endOfTurn = true;
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

            // save BotState changes
            await botStateSet.SaveAllChangesAsync(dc.Context, false, cancellationToken).ConfigureAwait(false);

            return new DialogManagerResult { TurnResult = turnResult };
        }

        /// <summary>
        /// Helper to send a trace activity with a memory snapshot of the active dialog DC. 
        /// </summary>
        private static async Task SendStateSnapshotTraceAsync(DialogContext dc, string traceLabel, CancellationToken cancellationToken)
        {
            // send trace of memory
            var snapshot = GetActiveDialogContext(dc).State.GetMemorySnapshot();
            var traceActivity = (Activity)Activity.CreateTraceActivity("BotState", "https://www.botframework.com/schemas/botState", snapshot, traceLabel);
            await dc.Context.SendActivityAsync(traceActivity, cancellationToken).ConfigureAwait(false);
        }

        // We should only cancel the current dialog stack if the EoC activity is coming from a parent (a root bot or another skill).
        // When the EoC is coming back from a child, we should just process that EoC normally through the 
        // dialog stack and let the child dialogs handle that.
        private static bool IsEocComingFromParent(ITurnContext turnContext)
        {
            // To determine the direction we check callerId property which is set to the parent bot
            // by the BotFrameworkHttpClient on outgoing requests.
            return !string.IsNullOrWhiteSpace(turnContext.Activity.CallerId);
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

        private async Task<DialogTurnResult> HandleSkillOnTurnAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            // the bot is running as a skill. 
            var turnContext = dc.Context;

            // Process remote cancellation
            if (turnContext.Activity.Type == ActivityTypes.EndOfConversation && dc.ActiveDialog != null && IsEocComingFromParent(turnContext))
            {
                // Handle remote cancellation request from parent.
                var activeDialogContext = GetActiveDialogContext(dc);

                var remoteCancelText = "Skill was canceled through an EndOfConversation activity from the parent.";
                await turnContext.TraceActivityAsync($"{typeof(Dialog).Name}.RunAsync()", label: $"{remoteCancelText}", cancellationToken: cancellationToken).ConfigureAwait(false);

                // Send cancellation message to the top dialog in the stack to ensure all the parents are canceled in the right order. 
                return await activeDialogContext.CancelAllDialogsAsync(true, cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            // Handle reprompt
            // Process a reprompt event sent from the parent.
            if (turnContext.Activity.Type == ActivityTypes.Event && turnContext.Activity.Name == DialogEvents.RepromptDialog)
            {
                if (dc.ActiveDialog == null)
                {
                    return new DialogTurnResult(DialogTurnStatus.Empty);
                }

                await dc.RepromptDialogAsync(cancellationToken).ConfigureAwait(false);
                return new DialogTurnResult(DialogTurnStatus.Waiting);
            }

            // Continue execution
            // - This will apply any queued up interruptions and execute the current/next step(s).
            var turnResult = await dc.ContinueDialogAsync(cancellationToken).ConfigureAwait(false);
            if (turnResult.Status == DialogTurnStatus.Empty)
            {
                // restart root dialog
                var startMessageText = $"Starting {_rootDialogId}.";
                await turnContext.TraceActivityAsync($"{typeof(Dialog).Name}.RunAsync()", label: $"{startMessageText}", cancellationToken: cancellationToken).ConfigureAwait(false);
                turnResult = await dc.BeginDialogAsync(_rootDialogId, cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            await SendStateSnapshotTraceAsync(dc, "Skill State", cancellationToken).ConfigureAwait(false);

            // Send end of conversation if it is completed or cancelled.
            if (turnResult.Status == DialogTurnStatus.Complete || turnResult.Status == DialogTurnStatus.Cancelled)
            {
                var endMessageText = $"Dialog {_rootDialogId} has **completed**. Sending EndOfConversation.";
                await turnContext.TraceActivityAsync($"{typeof(Dialog).Name}.RunAsync()", label: $"{endMessageText}", value: turnResult.Result, cancellationToken: cancellationToken).ConfigureAwait(false);

                // Send End of conversation at the end.
                var activity = new Activity(ActivityTypes.EndOfConversation) { Value = turnResult.Result };
                await turnContext.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
            }

            return turnResult;
        }

        private async Task<DialogTurnResult> HandleBotOnTurnAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            DialogTurnResult turnResult;

            // the bot is running as a root bot. 
            if (dc.ActiveDialog == null)
            {
                // start root dialog
                turnResult = await dc.BeginDialogAsync(_rootDialogId, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // Continue execution
                // - This will apply any queued up interruptions and execute the current/next step(s).
                turnResult = await dc.ContinueDialogAsync(cancellationToken).ConfigureAwait(false);

                if (turnResult.Status == DialogTurnStatus.Empty)
                {
                    // restart root dialog
                    turnResult = await dc.BeginDialogAsync(_rootDialogId, cancellationToken: cancellationToken).ConfigureAwait(false);
                }
            }

            await SendStateSnapshotTraceAsync(dc, "Bot State", cancellationToken).ConfigureAwait(false);

            return turnResult;
        }
    }
}
