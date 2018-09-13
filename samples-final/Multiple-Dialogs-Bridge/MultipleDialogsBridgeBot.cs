// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Bridge;
using Microsoft.Bot.Schema;
using Multiple_Dialogs_Bridge.Dialogs;

namespace Multiple_Dialogs_Bridge
{
    /// <summary>
    /// This bot hosts Bot Builder v3 dialgs in a v4 bot using Microsoft.Bot.Builder.Dialogs.Bridge.
    /// </summary>
    public class MultipleDialogsBridgeBot : IBot
    {
        private MultipleDialogsAccessors Accessors { get; }

        private DialogSet Dialogs { get; }

        public MultipleDialogsBridgeBot(MultipleDialogsAccessors accessors)
        {
            this.Accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            this.Dialogs = new DialogSet(this.Accessors.ConversationDialogState);
            var bridgeDialog = new BridgeDialog(this.Accessors.PrivateConversationState, this.Accessors.ConversationState, this.Accessors.UserState);
            this.Dialogs.Add(bridgeDialog);
        }

        /// <summary>
        /// This controls what happens when an activity gets sent to the bot.
        /// </summary>
        /// <param name="turnContext">Provides the <see cref="ITurnContext"/> for the turn of the bot.</param>
        /// <param name="cancellationToken" >(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>>A <see cref="Task"/> representing the operation result of the Turn operation.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            switch (turnContext.Activity.Type)
            {
                case ActivityTypes.Message:

                    var dialogContext = await this.Dialogs.CreateContextAsync(turnContext);
                    var results = await dialogContext.ContinueAsync();

                    if (!turnContext.Responded && results.Result == null && results.Status == DialogTurnStatus.Empty)
                    {
                        await dialogContext.BeginAsync(BridgeDialog.DialogId, Options.From(new RootDialog()));
                    }

                    break;
                case ActivityTypes.ConversationUpdate:
                    // Send a welcome message to the user and tell them what actions they may perform to use this bot
                    if (turnContext.Activity.MembersAdded.Any())
                    {
                        await SendWelcomeMessageAsync(turnContext, cancellationToken);
                    }

                    break;
            }
        }

        /// <summary>
        /// On a conversation update activity sent to the bot, the bot will
        /// send a message to the any new user(s) that were added.
        /// </summary>
        /// <param name="turnContext">Provides the <see cref="ITurnContext"/> for the turn of the bot.</param>
        /// <param name="cancellationToken" >(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>>A <see cref="Task"/> representing the operation result of the Turn operation.</returns>
        private static async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(
                        $"Welcome to MultipleDialogsBridgeBot {member.Name}." +
                        $" This bot demonstrates how to use v3 dialogs in a v4 bot.",
                        cancellationToken: cancellationToken);

                    await turnContext.SendActivityAsync(
                        "Say Hi to get started.",
                        cancellationToken: cancellationToken);
                }
            }
        }
    }
}
