// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class IntegrationBot : TeamsActivityHandler
    {
        private List<string> _activityIds;

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Text != null)
            {
                turnContext.Activity.RemoveRecipientMention();
                string actualText = turnContext.Activity.Text;
                if (!string.IsNullOrWhiteSpace(actualText))
                {
                    actualText = actualText.Trim();
                    await HandleBotCommand(turnContext, actualText, cancellationToken);
                }
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("App sent a message with empty text"), cancellationToken);
                if (turnContext.Activity.Value != null)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"but with value {JsonConvert.SerializeObject(turnContext.Activity.Value)}"), cancellationToken);
                }
            }
        }

        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            var adaptiveCardEditor = AdaptiveCardHelper.CreateAdaptiveCardEditor();

            return new MessagingExtensionActionResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo
                    {
                        Card = new Attachment
                        {
                            Content = adaptiveCardEditor,
                            ContentType = AdaptiveCard.ContentType,
                        },
                        Height = 450,
                        Width = 500,
                        Title = "Task Module Fetch Example",
                    },
                },
            };
        }

        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            var exampleData = JsonConvert.DeserializeObject<ExampleData>(action.Data.ToString());

            var adaptiveCard = AdaptiveCardHelper.CreateAdaptiveCard(exampleData);

            // a number of reasonable options here...

            // (1) send a message on a new conversation and return null (only works in group chats and teams)

            // THIS WILL WORK IF THE BOT IS INSTALLED. (GetMembers() will NOT throw if the bot is installed.)

            //var message = MessageFactory.Attachment(new Attachment { ContentType = AdaptiveCard.ContentType, Content = adaptiveCard });
            //var channelId = turnContext.Activity.TeamsGetChannelId();
            //await turnContext.TeamsCreateConversationAsync(channelId, message, cancellationToken);
            //return null;

            // (2) drop the content into the compose window ready for the user to send

            //return new MessagingExtensionActionResponse
            //{
            //    ComposeExtension = new MessagingExtensionResult
            //    {
            //        Type = "result",
            //        AttachmentLayout = "list",
            //        Attachments = new List<MessagingExtensionAttachment>
            //        {
            //            new MessagingExtensionAttachment
            //            {
            //                Content = adaptiveCard,
            //                ContentType = AdaptiveCard.ContentType,
            //            },
            //        },
            //    },
            //};

            // (3) start a preview flow

            return new MessagingExtensionActionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "botMessagePreview",
                    ActivityPreview = MessageFactory.Attachment(new Attachment
                    {
                        Content = adaptiveCard,
                        ContentType = AdaptiveCard.ContentType,
                    }) as Activity,
                },
            };
        }

        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionBotMessagePreviewEditAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            // The data has been returned to the bot in the action structure.
            var activityPreview = action.BotActivityPreview[0];
            var attachmentContent = activityPreview.Attachments[0].Content;
            var previewedCard = JsonConvert.DeserializeObject<AdaptiveCard>(attachmentContent.ToString(), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var exampleData = AdaptiveCardHelper.CreateExampleData(previewedCard);

            // This is a preview edit call and so this time we want to re-create the adaptive card editor.
            var adaptiveCardEditor = AdaptiveCardHelper.CreateAdaptiveCardEditor(exampleData);

            return new MessagingExtensionActionResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo
                    {
                        Card = new Attachment
                        {
                            Content = adaptiveCardEditor,
                            ContentType = AdaptiveCard.ContentType,
                        },
                        Height = 450,
                        Width = 500,
                        Title = "Task Module Fetch Example",
                    },
                },
            };
        }

        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionBotMessagePreviewSendAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            // The data has been returned to the bot in the action structure.
            var activityPreview = action.BotActivityPreview[0];
            var attachmentContent = activityPreview.Attachments[0].Content;
            var previewedCard = JsonConvert.DeserializeObject<AdaptiveCard>(attachmentContent.ToString(), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            var exampleData = AdaptiveCardHelper.CreateExampleData(previewedCard);

            // This is a send so we are done and we will create the adaptive card editor.
            var adaptiveCard = AdaptiveCardHelper.CreateAdaptiveCard(exampleData);

            var message = MessageFactory.Attachment(new Attachment { ContentType = AdaptiveCard.ContentType, Content = adaptiveCard });

            // THIS WILL WORK IF THE BOT IS INSTALLED. (GetMembers() will NOT throw if the bot is installed.)
            // (The application should fail gracefully.)
            var channelId = turnContext.Activity.TeamsGetChannelId();

            var conversationParameters = new ConversationParameters
            {
                IsGroup = true,
                ChannelData = new TeamsChannelData { Channel = new ChannelInfo(channelId) },
                Activity = (Activity)message,
            };

            var connectorClient = turnContext.TurnState.Get<IConnectorClient>();

            // This call does NOT send the outbound Activity is not being sent through the middleware stack.
            var conversationResourceResponse = await connectorClient.Conversations.CreateConversationAsync(conversationParameters, cancellationToken).ConfigureAwait(false);

            return null;
        }

        protected override async Task OnTeamsMessagingExtensionCardButtonClickedAsync(ITurnContext<IInvokeActivity> turnContext, JObject obj, CancellationToken cancellationToken)
        {
            // If the adaptive card was added to the compose window (by either the OnTeamsMessagingExtensionSubmitActionAsync or
            // OnTeamsMessagingExtensionBotMessagePreviewSendAsync handler's return values) the submit values will come in here.
            var reply = MessageFactory.Text("OnTeamsMessagingExtensionCardButtonClickedAsync Value: " + JsonConvert.SerializeObject(turnContext.Activity.Value));
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        private async Task HandleBotCommand(ITurnContext<IMessageActivity> turnContext, string actualText, CancellationToken cancellationToken)
        {
            switch (actualText.ToLowerInvariant())
            {
                case "delete":
                    await HandleDeleteActivities(turnContext, cancellationToken);
                    break;
                case "1":
                    await SendAdaptiveCard1(turnContext, cancellationToken);
                    break;
                case "2":
                    await SendAdaptiveCard2(turnContext, cancellationToken);
                    break;
                case "3":
                    await SendAdaptiveCard3(turnContext, cancellationToken);
                    break;
                case "hero":
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(Cards.GetHeroCard().ToAttachment()), cancellationToken);
                    break;
                case "thumbnail":
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(Cards.GetThumbnailCard().ToAttachment()), cancellationToken);
                    break;
                case "receipt":
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(Cards.GetReceiptCard().ToAttachment()), cancellationToken);
                    break;
                case "signin":
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(Cards.GetSigninCard().ToAttachment()), cancellationToken);
                    break;
                case "carousel":
                    // NOTE: if cards are NOT the same height in a carousel, Teams will instead display as AttachmentLayoutTypes.List
                    await turnContext.SendActivityAsync(
                        MessageFactory.Carousel(new[] { Cards.GetHeroCard().ToAttachment(), Cards.GetHeroCard().ToAttachment(), Cards.GetHeroCard().ToAttachment() }),
                        cancellationToken);
                    break;
                case "list":
                    // NOTE: MessageFactory.Attachment with multiple attachments will default to AttachmentLayoutTypes.List
                    await turnContext.SendActivityAsync(
                        MessageFactory.Attachment(new[] { Cards.GetHeroCard().ToAttachment(), Cards.GetHeroCard().ToAttachment(), Cards.GetHeroCard().ToAttachment() }),
                        cancellationToken);
                    break;
                case "o365":
                    await SendO365CardAttachment(turnContext, cancellationToken);
                    break;
                case "file":
                    await SendFileCard(turnContext, cancellationToken);
                    break;
                case "show members":
                    await ShowMembers(turnContext, cancellationToken);
                    break;
                case "show channels":
                    await ShowChannels(turnContext, cancellationToken);
                    break;
                case "show details":
                    await ShowDetails(turnContext, cancellationToken);
                    break;
                case "task module":
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(GetTaskModuleHeroCard()), cancellationToken);
                    break;
                default:
                    await SendMessageAndLogActivityId(turnContext, $"{turnContext.Activity.Text}", cancellationToken);
                    foreach (var activityId in _activityIds)
                    {
                        var newActivity = MessageFactory.Text(turnContext.Activity.Text);
                        newActivity.Id = activityId;
                        await turnContext.UpdateActivityAsync(newActivity, cancellationToken);
                    }
                    break;
            }
        }

        private async Task ShowDetails(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task ShowChannels(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task ShowMembers(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task SendFileCard(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task SendO365CardAttachment(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task SendAdaptiveCard3(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task SendAdaptiveCard2(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task SendAdaptiveCard1(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task HandleDeleteActivities(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var activityId in _activityIds)
            {
                await turnContext.DeleteActivityAsync(activityId, cancellationToken);
            }

            _activityIds.Clear();
        }

        private Attachment GetTaskModuleHeroCard()
        {
            return new HeroCard()
            {
                Title = "Task Module Invocation from Hero Card",
                Subtitle = "This is a hero card with a Task Module Action button.  Click the button to show an Adaptive Card within a Task Module.",
                Buttons = new List<CardAction>()
                    {
                        new TaskModuleAction("Adaptive Card", new { data = "adaptivecard" }),
                    },
            }.ToAttachment();
        }

        private async Task SendMessageAndLogActivityId(ITurnContext turnContext, string text, CancellationToken cancellationToken)
        {
            // We need to record the Activity Id from the Activity just sent in order to understand what the reaction is a reaction too. 
            var replyActivity = MessageFactory.Text(text);
            var resourceResponse = await turnContext.SendActivityAsync(replyActivity, cancellationToken);
            _activityIds.Add(resourceResponse.Id);
        }
    }
}
