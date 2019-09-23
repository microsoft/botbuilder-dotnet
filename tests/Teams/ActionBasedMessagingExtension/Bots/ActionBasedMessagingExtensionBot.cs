// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
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
            var reply = MessageFactory.Text("OnTeamsMessagingExtensionSubmitActionAsync MessagingExtensionAction: " + JsonConvert.SerializeObject(action));
            await turnContext.SendActivityAsync(reply, cancellationToken);

            var adaptiveCard = GetCardFromSubmitExampleData(JsonConvert.DeserializeObject<SubmitExampleData>(action.Data.ToString()));

            return new MessagingExtensionActionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    Attachments = new[] { new MessagingExtensionAttachment(AdaptiveCard.ContentType, content: adaptiveCard) },
                    AttachmentLayout = AttachmentLayoutTypes.List,
                },
            };
        }

        protected override async Task OnTeamsMessagingExtensionCardButtonClickedAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("OnTeamsMessagingExtensionCardButtonClickedAsync Value: " + JsonConvert.SerializeObject(turnContext.Activity.Value));
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        private AdaptiveCard GetCardFromSubmitExampleData(SubmitExampleData data)
        {
            var adaptiveCard = new AdaptiveCard();
            adaptiveCard.Body.Add(new AdaptiveTextBlock("Adaptive Card from Parameters") { Weight = AdaptiveTextWeight.Bolder });
            adaptiveCard.Body.Add(new AdaptiveTextBlock($"{ data.Question }"));
            adaptiveCard.Body.Add(new AdaptiveTextInput() { Id = "Answer", Placeholder = "Answer here..." });
            var choices = new List<AdaptiveChoice>()
            {
                new AdaptiveChoice() { Title = data.Option1, Value = data.Option1 },
                new AdaptiveChoice() { Title = data.Option2, Value = data.Option2 },
                new AdaptiveChoice() { Title = data.Option3, Value = data.Option3 },
            };
            adaptiveCard.Body.Add(new AdaptiveChoiceSetInput() { Type = AdaptiveChoiceSetInput.TypeName, Id = "Choices", IsMultiSelect = bool.Parse(data.MultiSelect), Choices = choices });
            adaptiveCard.Actions.Add(new AdaptiveSubmitAction { Type = AdaptiveSubmitAction.TypeName, Title = "Submit", Data = new JObject { { "submitLocation", "messagingExtensionSubmit" } } });

            return adaptiveCard;
        }

        private class SubmitExampleData
        {
            public string id { get; set; }

            public string Question { get; set; }

            public string MultiSelect { get; set; }

            public string Option1 { get; set; }

            public string Option2 { get; set; }

            public string Option3 { get; set; }
        }
    }
}
