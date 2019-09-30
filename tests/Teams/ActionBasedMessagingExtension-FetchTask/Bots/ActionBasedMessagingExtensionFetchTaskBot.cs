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

            return new MessagingExtensionActionResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo()
                    {
                        Card = new Attachment
                        {
                            Content = GetTaskModuleAdaptiveCard(),
                            ContentType = AdaptiveCard.ContentType,
                        },
                        Height = 450,
                        Width = 450,
                        Title = "Task Module Example",
                    },
                },
            };
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
                    Text= "Testing Action Based Messaging Extension",
                    AttachmentLayout = AttachmentLayoutTypes.List,
                },
            };
        }

        protected override async Task OnTeamsMessagingExtensionCardButtonClickedAsync(ITurnContext<IInvokeActivity> turnContext, JObject obj, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("OnTeamsMessagingExtensionCardButtonClickedAsync Value: " + JsonConvert.SerializeObject(turnContext.Activity.Value));
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        private AdaptiveCard GetTaskModuleAdaptiveCard()
        {
            var adaptiveCard = new AdaptiveCard();
            adaptiveCard.Body.Add(new AdaptiveTextBlock("This is an Adaptive Card within a Task Module") { Weight = AdaptiveTextWeight.Bolder });
            adaptiveCard.Body.Add(new AdaptiveTextBlock("Enter text for Question:"));
            adaptiveCard.Body.Add(new AdaptiveTextInput() { Id = "Question", Placeholder = "Question text here" });
            adaptiveCard.Body.Add(new AdaptiveTextBlock("Options for Question:"));
            adaptiveCard.Body.Add(new AdaptiveTextBlock("Is Multi-Select:"));
            var choices = new List<AdaptiveChoice>()
            {
                new AdaptiveChoice() { Title = "True", Value = "true" },
                new AdaptiveChoice() { Title = "False", Value = "false" },
            };
            adaptiveCard.Body.Add(new AdaptiveChoiceSetInput() { Type = AdaptiveChoiceSetInput.TypeName, Id = "MultiSelect", Value = "true", IsMultiSelect = false, Choices = choices });

            adaptiveCard.Body.Add(new AdaptiveTextInput() { Id = "Option1", Placeholder = "Option 1 here" });
            adaptiveCard.Body.Add(new AdaptiveTextInput() { Id = "Option2", Placeholder = "Option 2 here" });
            adaptiveCard.Body.Add(new AdaptiveTextInput() { Id = "Option3", Placeholder = "Option 3 here" });

            adaptiveCard.Actions.Add(new AdaptiveSubmitAction { Type = "Action.Submit", Title = "Submit", Data = new JObject { { "submitLocation", "messagingExtensionFetchTask" } } });
            return adaptiveCard;
        }

        private AdaptiveCard GetCardFromSubmitExampleData(SubmitExampleData data)
        {
            var adaptiveCard = new AdaptiveCard();
            adaptiveCard.Body.Add(new AdaptiveTextBlock("Adaptive Card from Task Module") { Weight = AdaptiveTextWeight.Bolder });
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
            public string SubmitLocation { get; set; }

            public string Question { get; set; }

            public string MultiSelect { get; set; }

            public string Option1 { get; set; }

            public string Option2 { get; set; }

            public string Option3 { get; set; }
        }
    }
}
