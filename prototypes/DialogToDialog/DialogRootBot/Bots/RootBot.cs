// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DialogRootBot.Bots
{
    // This IBot implementation can run any type of Dialog. The use of type parameterization is to allows multiple different bots
    // to be run at different endpoints within the same project. This can be achieved by defining distinct Controller types
    // each with dependency on distinct IBot types, this way ASP Dependency Injection can glue everything together without ambiguity.
    // The ConversationState is used by the Dialog system. The UserState isn't, however, it might have been used in a Dialog implementation,
    // and the requirement is that all BotState objects are saved at the end of a turn.
    public class RootBot<T> : ActivityHandler
        where T : Dialog
    {
        private readonly BotState _conversationState;
        private readonly ILogger _logger;
        private readonly Dialog _mainDialog;

        public RootBot(ConversationState conversationState, T mainDialog, ILogger<RootBot<T>> logger)
        {
            _conversationState = conversationState;
            _mainDialog = mainDialog;
            _logger = logger;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Run the Dialog the activity Activity.
            if (turnContext.Activity.Type != ActivityTypes.ConversationUpdate)
            {
                await _mainDialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
            }
            else
            {
                await base.OnTurnAsync(turnContext, cancellationToken);
            }

            // Save any state changes that might have occured during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                // Greet anyone that was not the target (recipient) of this message.
                // To learn more about Adaptive Cards, see https://aka.ms/msbot-adaptivecards for more details.
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var welcomeCard = CreateAdaptiveCardAttachment();
                    var response = MessageFactory.Attachment(welcomeCard);
                    await turnContext.SendActivityAsync(response, cancellationToken);
                    var dialogSet = new DialogSet(_conversationState.CreateProperty<DialogState>("DialogState")) { TelemetryClient = _mainDialog.TelemetryClient };
                    dialogSet.Add(_mainDialog);

                    var dialogContext = await dialogSet.CreateContextAsync(turnContext, cancellationToken).ConfigureAwait(false);
                    var results = await dialogContext.ContinueDialogAsync(cancellationToken).ConfigureAwait(false);
                    if (results.Status == DialogTurnStatus.Empty)
                    {
                        results = await dialogContext.BeginDialogAsync(_mainDialog.Id, null, cancellationToken).ConfigureAwait(false);
                    }

                    DialogTurnResult temp = results;
                }
            }
        }

        // Load attachment from embedded resource.
        private Attachment CreateAdaptiveCardAttachment()
        {
            var cardResourcePath = "DialogRootBot.Cards.welcomeCard.json";

            using (var stream = GetType().Assembly.GetManifestResourceStream(cardResourcePath))
            {
                using (var reader = new StreamReader(stream))
                {
                    var adaptiveCard = reader.ReadToEnd();
                    return new Attachment() { ContentType = "application/vnd.microsoft.card.adaptive", Content = JsonConvert.DeserializeObject(adaptiveCard), };
                }
            }
        }
    }
}
