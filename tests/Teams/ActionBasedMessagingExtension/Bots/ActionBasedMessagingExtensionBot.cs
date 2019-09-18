// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class ActionBasedMessagingExtensionBot : TeamsActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text($"echo: {turnContext.Activity.Text}"), cancellationToken);
        }

        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            var response = action;
            var t = "hello";
            // Need to convert action.data to a base class that has Id:
            // 
            switch (action.CommandId)
            {
                case "createCard":
                    return HandleCreateCardCommands(turnContext, action);

                case "shareMessage":
                    return HandleShareMessageCommand(turnContext, action);
                default:
                    return new MessagingExtensionActionResponse();
            }
        }

        private MessagingExtensionActionResponse HandleCreateCardCommands(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action)
        {
            var actionData = (action.Data as JObject).ToObject<CreateCardData>();
            var response = new MessagingExtensionActionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    AttachmentLayout = "list",
                    Type = "list",
                },
            };

            var card = new HeroCard();

            var attachments = new List<MessagingExtensionAttachment>();
            attachments.Add(new MessagingExtensionAttachment
            {
                Content = card,
                ContentType = HeroCard.ContentType,
                Preview = card.ToAttachment(),
            });

            return response;
        }

        private MessagingExtensionActionResponse HandleShareMessageCommand(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action)
        {
            var response = new MessagingExtensionActionResponse();

            return response;
        }
    }
}
