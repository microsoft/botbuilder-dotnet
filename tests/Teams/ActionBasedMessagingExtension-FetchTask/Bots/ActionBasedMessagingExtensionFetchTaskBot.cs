// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class ActionBasedMessagingExtensionFetchTaskBot : TeamsActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text($"echo: {turnContext.Activity.Text}"), cancellationToken);
        }

        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("OnTeamsMessagingExtensionFetchTaskAsync MessagingExtensionQuery: " + JsonConvert.SerializeObject(query));
            await turnContext.SendActivityAsync(reply, cancellationToken);

            return AdaptiveCardHelper.CreateTaskModuleAdaptiveCardResponse();
        }

        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("OnTeamsMessagingExtensionSubmitActionAsync MessagingExtensionAction: " + JsonConvert.SerializeObject(action));
            await turnContext.SendActivityAsync(reply, cancellationToken);

            var submittedData = JsonConvert.DeserializeObject<SubmitExampleData>(action.Data.ToString());
            var adaptiveCard = submittedData.ToAdaptiveCard();
            return adaptiveCard.ToMessagingExtensionBotMessagePreviewResponse();
        }

        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionBotMessagePreviewEditAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("OnTeamsMessagingExtensionBotMessagePreviewEditAsync MessagingExtensionAction: " + JsonConvert.SerializeObject(action));
            await turnContext.SendActivityAsync(reply, cancellationToken);

            var submitData = action.ToSubmitExampleData();
            return AdaptiveCardHelper.CreateTaskModuleAdaptiveCardResponse(
                                                        submitData.Question,
                                                        bool.Parse(submitData.MultiSelect),
                                                        submitData.Option1,
                                                        submitData.Option2,
                                                        submitData.Option3);
        }

        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionBotMessagePreviewSendAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("OnTeamsMessagingExtensionBotMessagePreviewSendAsync MessagingExtensionAction: " + JsonConvert.SerializeObject(action));
            await turnContext.SendActivityAsync(reply, cancellationToken);

            var submitData = action.ToSubmitExampleData();
            var adaptiveCard = submitData.ToAdaptiveCard();
            return adaptiveCard.ToComposeExtensionResultResponse();
        }

        protected override async Task OnTeamsMessagingExtensionCardButtonClickedAsync(ITurnContext<IInvokeActivity> turnContext, JObject obj, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("OnTeamsMessagingExtensionCardButtonClickedAsync Value: " + JsonConvert.SerializeObject(turnContext.Activity.Value));
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
    }
}
