// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
        private ActivityLog _log;
        private List<string> _activityIds;
        private readonly IHttpClientFactory _clientFactory;

        public IntegrationBot(ActivityLog log, List<string> activityIds, IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            _log = log;
            _activityIds = activityIds;
        }

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

            var attachments = new MessagingExtensionAttachment(AdaptiveCard.ContentType, null, adaptiveCard);
            var result = new MessagingExtensionResult(AttachmentLayoutTypes.List, "result", new[] { attachments }, null);

            return new MessagingExtensionActionResponse()
            {
                ComposeExtension = result,
            };
        }

        protected override async Task OnTeamsMessagingExtensionCardButtonClickedAsync(ITurnContext<IInvokeActivity> turnContext, JObject obj, CancellationToken cancellationToken)
        {
            // If the adaptive card was added to the compose window (by either the OnTeamsMessagingExtensionSubmitActionAsync or
            // OnTeamsMessagingExtensionBotMessagePreviewSendAsync handler's return values) the submit values will come in here.
            var reply = MessageFactory.Text("OnTeamsMessagingExtensionCardButtonClickedAsync Value: " + JsonConvert.SerializeObject(turnContext.Activity.Value));
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("handleTeamsTaskModuleFetchAsync TaskModuleRequest: " + JsonConvert.SerializeObject(taskModuleRequest));
            await turnContext.SendActivityAsync(reply, cancellationToken);

            var adaptiveCard = new AdaptiveCard();
            adaptiveCard.Body.Add(new AdaptiveTextBlock("This is an Adaptive Card within a Task Module"));
            adaptiveCard.Actions.Add(new AdaptiveSubmitAction { Type = "Action.Submit", Title = "Action.Submit", Data = new JObject { { "submitLocation", "taskModule" } } });

            return new TaskModuleResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo()
                    {
                        Card = adaptiveCard.ToAttachment(),
                        Height = 200,
                        Width = 400,
                        Title = "Task Module Example",
                    },
                },
            };
        }

        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text($"handleTeamsTaskModuleFetchAsync Value: { JsonConvert.SerializeObject(taskModuleRequest) }"), cancellationToken);

            return new TaskModuleResponse
            {
                Task = new TaskModuleMessageResponse()
                {
                    Value = "Thanks!",
                },
            };
        }

        protected override async Task<InvokeResponse> OnTeamsCardActionInvokeAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            var value = turnContext.Activity.Value.ToString().Replace("\r\n", string.Empty).Replace(" ", string.Empty).Trim();
            await turnContext.SendActivityAsync(MessageFactory.Text($"handleTeamsCardActionInvoke value: {value}"), cancellationToken);
            return new InvokeResponse() { Status = 200 };
        }

        protected override async Task OnTeamsChannelRenamedAsync(ChannelInfo channelInfo, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var heroCard = new HeroCard(text: $"{channelInfo.Name} is the new Channel name");
            await turnContext.SendActivityAsync(MessageFactory.Attachment(heroCard.ToAttachment()), cancellationToken);
        }

        protected override async Task OnTeamsChannelCreatedAsync(ChannelInfo channelInfo, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var heroCard = new HeroCard(text: $"{channelInfo.Name} is the Channel created");
            await turnContext.SendActivityAsync(MessageFactory.Attachment(heroCard.ToAttachment()), cancellationToken);
        }

        protected override async Task OnTeamsChannelDeletedAsync(ChannelInfo channelInfo, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var heroCard = new HeroCard(text: $"{channelInfo.Name} is the Channel deleted");
            await turnContext.SendActivityAsync(MessageFactory.Attachment(heroCard.ToAttachment()), cancellationToken);
        }

        protected override async Task OnTeamsTeamRenamedAsync(TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var heroCard = new HeroCard(text: $"{teamInfo.Name} is the new Team name");
            await turnContext.SendActivityAsync(MessageFactory.Attachment(heroCard.ToAttachment()), cancellationToken);
        }

        protected override async Task OnTeamsMembersAddedAsync(IList<TeamsChannelAccount> membersAdded, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var heroCard = new HeroCard(text: $"{string.Join(' ', membersAdded.Select(member => member.Id))} joined {teamInfo.Name}");
            await turnContext.SendActivityAsync(MessageFactory.Attachment(heroCard.ToAttachment()), cancellationToken);
        }

        protected override async Task OnTeamsMembersRemovedAsync(IList<TeamsChannelAccount> membersRemoved, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var heroCard = new HeroCard(text: $"{string.Join(' ', membersRemoved.Select(member => member.Id))} removed from {teamInfo.Name}");
            await turnContext.SendActivityAsync(MessageFactory.Attachment(heroCard.ToAttachment()), cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var heroCard = new HeroCard(text: $"{string.Join(' ', membersAdded.Select(member => member.Id))} joined {turnContext.Activity.Conversation.ConversationType}");
            await turnContext.SendActivityAsync(MessageFactory.Attachment(heroCard.ToAttachment()), cancellationToken);
        }

        protected override async Task OnMembersRemovedAsync(IList<ChannelAccount> membersRemoved, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var heroCard = new HeroCard(text: $"{string.Join(' ', membersRemoved.Select(member => member.Id))} removed from {turnContext.Activity.Conversation.ConversationType}");
            await turnContext.SendActivityAsync(MessageFactory.Attachment(heroCard.ToAttachment()), cancellationToken);
        }

        protected override async Task<MessagingExtensionResponse> OnTeamsAppBasedLinkQueryAsync(ITurnContext<IInvokeActivity> turnContext, AppBasedLinkQuery query, CancellationToken cancellationToken)
        {
            var heroCard = new ThumbnailCard
            {
                Title = "Thumbnail Card",
                Text = query.Url,
                Images = new List<CardImage> { new CardImage("https://raw.githubusercontent.com/microsoft/botframework-sdk/master/icon.png") },
            };

            var attachments = new MessagingExtensionAttachment(HeroCard.ContentType, null, heroCard);
            var result = new MessagingExtensionResult(AttachmentLayoutTypes.List, "result", new[] { attachments }, null, "test unfurl");

            return new MessagingExtensionResponse(result);
        }

        protected override async Task OnReactionsAddedAsync(IList<MessageReaction> messageReactions, ITurnContext<IMessageReactionActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var reaction in messageReactions)
            {
                // The ReplyToId property of the inbound MessageReaction Activity will correspond to a Message Activity that was previously sent from this bot.
                var activity = await _log.Find(turnContext.Activity.ReplyToId);
                if (activity == null)
                {
                    // If we had sent the message from the error handler we wouldn't have recorded the Activity Id and so we shouldn't expect to see it in the log.
                    await SendMessageAndLogActivityIdAsync(turnContext, $"Activity {turnContext.Activity.ReplyToId} not found in the log.", cancellationToken);
                }

                await SendMessageAndLogActivityIdAsync(turnContext, $"You added '{reaction.Type}' regarding '{activity.Text}'", cancellationToken);
            }
        }

        protected override async Task OnReactionsRemovedAsync(IList<MessageReaction> messageReactions, ITurnContext<IMessageReactionActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var reaction in messageReactions)
            {
                // The ReplyToId property of the inbound MessageReaction Activity will correspond to a Message Activity that was previously sent from this bot.
                var activity = await _log.Find(turnContext.Activity.ReplyToId);
                if (activity == null)
                {
                    // If we had sent the message from the error handler we wouldn't have recorded the Activity Id and so we shouldn't expect to see it in the log.
                    await SendMessageAndLogActivityIdAsync(turnContext, $"Activity {turnContext.Activity.ReplyToId} not found in the log.", cancellationToken);
                }

                await SendMessageAndLogActivityIdAsync(turnContext, $"You removed '{reaction.Type}' regarding '{activity.Text}'", cancellationToken);
            }
        }

        protected override async Task<MessagingExtensionResponse> OnTeamsMessagingExtensionConfigurationQuerySettingUrlAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            var messageExtensionResponse = new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "config",
                    SuggestedActions = new MessagingExtensionSuggestedAction
                    {
                        Actions = new List<CardAction>
                        {
                            new CardAction
                            {
                                Type = ActionTypes.OpenUrl,
                                Value = "https://teamssettingspagescenario.azurewebsites.net",
                            },
                        },
                    },
                },
            };

            return messageExtensionResponse;
        }

        /// <inheritdoc/>
        protected override async Task OnTeamsMessagingExtensionConfigurationSettingAsync(ITurnContext<IInvokeActivity> turnContext, JObject settings, CancellationToken cancellationToken)
        {
            // This event is fired when the settings page is submitted
            var reply = MessageFactory.Text($"handleTeamsMessagingExtensionConfigurationSetting event fired with {settings}");
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        protected override async Task OnTeamsO365ConnectorCardActionAsync(ITurnContext<IInvokeActivity> turnContext, O365ConnectorCardActionQuery query, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text($"O365ConnectorCardActionQuery event value: {JsonConvert.SerializeObject(query)}"));
        }

        protected override async Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            var searchQuery = query.Parameters[0].Value as string;
            var messagingExtensionResponse = new MessagingExtensionResponse();

            messagingExtensionResponse.ComposeExtension = CreateMessagingExtensionResult(new List<MessagingExtensionAttachment>
            {
                CreateSearchResultAttachment(searchQuery),
                CreateDummySearchResultAttachment(),
                CreateSelectItemsResultAttachment(searchQuery)
            });

            return messagingExtensionResponse;
        }

        protected override async Task<MessagingExtensionResponse> OnTeamsMessagingExtensionSelectItemAsync(ITurnContext<IInvokeActivity> turnContext, JObject query, CancellationToken cancellationToken)
        {
            var searchQuery = query.ToObject<SearchQuery>();
            var bfLogo = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQtB3AwMUeNoq4gUBGe6Ocj8kyh3bXa9ZbV7u1fVKQoyKFHdkqU";
            var card = new HeroCard
            {
                Title = $"You selected a search result!",
                Text = $"You searched for \"{searchQuery.Query}\"",
                Images = new List<CardImage>
                    {
                        new CardImage { Url = bfLogo }
                    }
            };

            var attachment = new MessagingExtensionAttachment
            {
                ContentType = HeroCard.ContentType,
                Content = card
            };

            var messagingExtensionResponse = new MessagingExtensionResponse();
            messagingExtensionResponse.ComposeExtension = CreateMessagingExtensionResult(new List<MessagingExtensionAttachment> { attachment });
            return messagingExtensionResponse;
        }

        protected override async Task OnTeamsFileConsentAcceptAsync(ITurnContext<IInvokeActivity> turnContext, FileConsentCardResponse fileConsentCardResponse, CancellationToken cancellationToken)
        {
            try
            {
                JToken context = JObject.FromObject(fileConsentCardResponse.Context);

                string filePath = Path.Combine("Files", context["filename"].ToString());
                long fileSize = new FileInfo(filePath).Length;
                var client = _clientFactory.CreateClient();
                using (var fileStream = File.OpenRead(filePath))
                {
                    var fileContent = new StreamContent(fileStream);
                    fileContent.Headers.ContentLength = fileSize;
                    fileContent.Headers.ContentRange = new ContentRangeHeaderValue(0, fileSize - 1, fileSize);
                    await client.PutAsync(fileConsentCardResponse.UploadInfo.UploadUrl, fileContent, cancellationToken);
                }

                await FileUploadCompletedAsync(turnContext, fileConsentCardResponse, cancellationToken);
            }
            catch (Exception e)
            {
                await FileUploadFailedAsync(turnContext, e.ToString(), cancellationToken);
            }
        }

        protected override async Task OnTeamsFileConsentDeclineAsync(ITurnContext<IInvokeActivity> turnContext, FileConsentCardResponse fileConsentCardResponse, CancellationToken cancellationToken)
        {
            JToken context = JObject.FromObject(fileConsentCardResponse.Context);

            var reply = ((Activity)turnContext.Activity).CreateReply();
            reply.TextFormat = "xml";
            reply.Text = $"Declined. We won't upload file <b>{context["filename"]}</b>.";
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        private async Task FileUploadCompletedAsync(ITurnContext turnContext, FileConsentCardResponse fileConsentCardResponse, CancellationToken cancellationToken)
        {
            var downloadCard = new FileInfoCard
            {
                UniqueId = fileConsentCardResponse.UploadInfo.UniqueId,
                FileType = fileConsentCardResponse.UploadInfo.FileType,
            };

            var asAttachment = new Attachment
            {
                Content = downloadCard,
                ContentType = FileInfoCard.ContentType,
                Name = fileConsentCardResponse.UploadInfo.Name,
                ContentUrl = fileConsentCardResponse.UploadInfo.ContentUrl,
            };

            var reply = turnContext.Activity.CreateReply();
            reply.TextFormat = "xml";
            reply.Text = $"<b>File uploaded.</b> Your file <b>{fileConsentCardResponse.UploadInfo.Name}</b> is ready to download";
            reply.Attachments = new List<Attachment> { asAttachment };

            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        private async Task FileUploadFailedAsync(ITurnContext turnContext, string error, CancellationToken cancellationToken)
        {
            var reply = turnContext.Activity.CreateReply();
            reply.TextFormat = "xml";
            reply.Text = $"<b>File upload failed.</b> Error: <pre>{error}</pre>";
            await turnContext.SendActivityAsync(reply, cancellationToken);
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

        private Attachment GetTaskModuleAdaptiveCard()
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion("1.0"))
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveTextBlock() { Text = "Enter Text Here" },
                    new AdaptiveTextInput()
                    {
                        Id = "usertext",
                        Spacing = AdaptiveSpacing.None,
                        IsMultiline = true,
                        Placeholder = "add some text and submit",
                    },
                },
                Actions = new List<AdaptiveAction>()
                {
                    new AdaptiveSubmitAction() { Title = "Submit" },
                },
            };

            return new Attachment
            {
                Content = card,
                ContentType = AdaptiveCards.AdaptiveCard.ContentType,
            };
        }

        private MessagingExtensionResult CreateMessagingExtensionResult(List<MessagingExtensionAttachment> attachments)
        {
            return new MessagingExtensionResult
            {
                Type = "result",
                AttachmentLayout = "list",
                Attachments = attachments
            };
        }

        private MessagingExtensionAttachment CreateSearchResultAttachment(string searchQuery)
        {
            var cardText = $"You said \"{searchQuery}\"";
            var bfLogo = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQtB3AwMUeNoq4gUBGe6Ocj8kyh3bXa9ZbV7u1fVKQoyKFHdkqU";

            var button = new CardAction
            {
                Type = "openUrl",
                Title = "Click for more Information",
                Value = "https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/bots/bots-overview"
            };

            var images = new List<CardImage>();
            images.Add(new CardImage(bfLogo));
            var buttons = new List<CardAction>();
            buttons.Add(button);

            var heroCard = new HeroCard("You searched for:", text: cardText, images: images, buttons: buttons);
            var preview = new HeroCard("You searched for:", text: cardText, images: images);

            return new MessagingExtensionAttachment
            {
                ContentType = HeroCard.ContentType,
                Content = heroCard,
                Preview = preview.ToAttachment()
            };
        }

        private MessagingExtensionAttachment CreateDummySearchResultAttachment()
        {
            var cardText = "https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/bots/bots-overview";
            var bfLogo = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQtB3AwMUeNoq4gUBGe6Ocj8kyh3bXa9ZbV7u1fVKQoyKFHdkqU";

            var button = new CardAction
            {
                Type = "openUrl",
                Title = "Click for more Information",
                Value = "https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/bots/bots-overview"
            };

            var images = new List<CardImage>();
            images.Add(new CardImage(bfLogo));
            var buttons = new List<CardAction>();
            buttons.Add(button);

            var heroCard = new HeroCard("Learn more about Teams:", text: cardText, images: images, buttons: buttons);
            var preview = new HeroCard("Learn more about Teams:", text: cardText, images: images);

            return new MessagingExtensionAttachment
            {
                ContentType = HeroCard.ContentType,
                Content = heroCard,
                Preview = preview.ToAttachment()
            };
        }

        private MessagingExtensionAttachment CreateSelectItemsResultAttachment(string searchQuery)
        {
            var bfLogo = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQtB3AwMUeNoq4gUBGe6Ocj8kyh3bXa9ZbV7u1fVKQoyKFHdkqU";
            var cardText = $"You said \"{searchQuery}\"";

            var button = new CardAction
            {
                Type = "openUrl",
                Title = "Click for more Information",
                Value = "https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/bots/bots-overview"
            };

            var images = new List<CardImage>();
            images.Add(new CardImage(bfLogo));
            var buttons = new List<CardAction>();
            buttons.Add(button);
            var selectItemTap = new CardAction
            {
                Type = "invoke",
                Value = new SearchQuery { Query = searchQuery }
            };

            var heroCard = new HeroCard(cardText, text: cardText, images: images);
            var preview = new HeroCard(cardText, text: cardText, images: images, tap: selectItemTap);

            return new MessagingExtensionAttachment
            {
                ContentType = HeroCard.ContentType,
                Content = heroCard,
                Preview = preview.ToAttachment()
            };
        }

        private async Task HandleBotCommand(ITurnContext<IMessageActivity> turnContext, string actualText, CancellationToken cancellationToken)
        {
            switch (actualText.ToLowerInvariant())
            {
                case "delete":
                    await HandleDeleteActivitiesAsync(turnContext, cancellationToken);
                    break;
                case "update":
                    await HandleUpdateActivitiesAsync(turnContext, cancellationToken);
                    break;
                case "1":
                    await SendAdaptiveCard1Async(turnContext, cancellationToken);
                    break;
                case "2":
                    await SendAdaptiveCard2Async(turnContext, cancellationToken);
                    break;
                case "3":
                    await SendAdaptiveCard3Async(turnContext, cancellationToken);
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
                    await SendO365CardAttachmentAsync(turnContext, cancellationToken);
                    break;
                case "file":
                    await SendFileCardAsync(turnContext, cancellationToken);
                    break;
                case "show members":
                    await ShowMembersAsync(turnContext, cancellationToken);
                    break;
                case "show channels":
                    await ShowChannelsAsync(turnContext, cancellationToken);
                    break;
                case "show details":
                    await ShowDetailsAsync(turnContext, cancellationToken);
                    break;
                case "task module":
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(this.GetTaskModuleHeroCard()), cancellationToken);
                    break;
                default:
                    await SendMessageAndLogActivityIdAsync(turnContext, $"{turnContext.Activity.Text}", cancellationToken);
                    break;
            }
        }

        private async Task ShowDetailsAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var teamId = turnContext.Activity.TeamsGetTeamInfo().Id;
            var teamDetails = await TeamsInfo.GetTeamDetailsAsync(turnContext, teamId, cancellationToken);
            await SendMessageAndLogActivityIdAsync(turnContext, $"The team name is {teamDetails.Name}. The team ID is {teamDetails.Id}. The ADD GroupID is {teamDetails.AadGroupId}.", cancellationToken);
        }

        private async Task ShowMembersAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await ShowMembersAsync(turnContext, await TeamsInfo.GetMembersAsync(turnContext, cancellationToken), cancellationToken);
        }

        private async Task ShowChannelsAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var teamId = turnContext.Activity.TeamsGetTeamInfo().Id;

            var channels = await TeamsInfo.GetTeamChannelsAsync(turnContext, teamId, cancellationToken);

            var replyActivity = MessageFactory.Text($"Total of {channels.Count} channels are currently in team");

            await turnContext.SendActivityAsync(replyActivity);

            var messages = channels.Select(channel => $"{channel.Id} --> {channel.Name}");

            await SendInBatchesAsync(turnContext, messages, cancellationToken);
        }

        private async Task ShowMembersAsync(ITurnContext<IMessageActivity> turnContext, IEnumerable<TeamsChannelAccount> teamsChannelAccounts, CancellationToken cancellationToken)
        {
            var replyActivity = MessageFactory.Text($"Total of {teamsChannelAccounts.Count()} members are currently in team");
            await turnContext.SendActivityAsync(replyActivity);

            var messages = teamsChannelAccounts
                .Select(teamsChannelAccount => $"{teamsChannelAccount.AadObjectId} --> {teamsChannelAccount.Name} -->  {teamsChannelAccount.UserPrincipalName}");

            await SendInBatchesAsync(turnContext, messages, cancellationToken);
        }

        private async Task SendInBatchesAsync(ITurnContext<IMessageActivity> turnContext, IEnumerable<string> messages, CancellationToken cancellationToken)
        {
            var batch = new List<string>();
            foreach (var msg in messages)
            {
                batch.Add(msg);

                if (batch.Count == 10)
                {
                    await SendMessageAndLogActivityIdAsync(turnContext, string.Join("<br>", batch), cancellationToken);
                    batch.Clear();
                }
            }

            if (batch.Count > 0)
            {
                await SendMessageAndLogActivityIdAsync(turnContext, string.Join("<br>", batch), cancellationToken);
            }
        }

        private async Task SendFileCardAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            string filename = "teams-logo.png";
            string filePath = Path.Combine("Files", filename);
            long fileSize = new FileInfo(filePath).Length;
            await SendFileCardAsync(turnContext, filename, fileSize, cancellationToken);
        }

        private async Task SendFileCardAsync(ITurnContext turnContext, string filename, long filesize, CancellationToken cancellationToken)
        {
            var consentContext = new Dictionary<string, string>
            {
                { "filename", filename },
            };

            var fileCard = new FileConsentCard
            {
                Description = "This is the file I want to send you",
                SizeInBytes = filesize,
                AcceptContext = consentContext,
                DeclineContext = consentContext,
            };

            var asAttachment = new Attachment
            {
                Content = fileCard,
                ContentType = FileConsentCard.ContentType,
                Name = filename,
            };

            var replyActivity = turnContext.Activity.CreateReply();
            replyActivity.Attachments = new List<Attachment>() { asAttachment };

            await turnContext.SendActivityAsync(replyActivity, cancellationToken);
        }

        private async Task SendO365CardAttachmentAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var card = O365Cards.CreateSampleO365ConnectorCard();
            var cardAttachment = new Attachment
            {
                Content = card,
                ContentType = O365ConnectorCard.ContentType,
            };

            await turnContext.SendActivityAsync(MessageFactory.Attachment(cardAttachment));
        }

        private async Task SendAdaptiveCard3Async(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var adaptiveCard = new AdaptiveCard();
            adaptiveCard.Body.Add(new AdaptiveTextBlock("Bot Builder actions"));
            adaptiveCard.Body.Add(new AdaptiveTextInput { Id = "x" });
            adaptiveCard.Actions.Add(new AdaptiveSubmitAction { Type = "Action.Submit", Title = "Action.Submit", Data = new JObject { { "key", "value" } } });

            var replyActivity = MessageFactory.Attachment(adaptiveCard.ToAttachment());
            await turnContext.SendActivityAsync(replyActivity, cancellationToken);
        }

        private async Task SendAdaptiveCard2Async(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var taskModuleAction = new TaskModuleAction("Launch Task Module", @"{ ""hiddenKey"": ""hidden value from task module launcher"" }");

            var adaptiveCard = new AdaptiveCard();
            adaptiveCard.Body.Add(new AdaptiveTextBlock("Task Module Adaptive Card"));
            adaptiveCard.Actions.Add(taskModuleAction.ToAdaptiveCardAction());

            var replyActivity = MessageFactory.Attachment(adaptiveCard.ToAttachment());
            await turnContext.SendActivityAsync(replyActivity, cancellationToken);
        }

        private async Task SendAdaptiveCard1Async(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var adaptiveCard = new AdaptiveCard();
            adaptiveCard.Body.Add(new AdaptiveTextBlock("Bot Builder actions"));

            var action1 = new CardAction(ActionTypes.ImBack, "imBack", null, null, null, "text");
            var action2 = new CardAction(ActionTypes.MessageBack, "message back", null, null, null, JObject.Parse(@"{ ""key"" : ""value"" }"));
            var action3 = new CardAction(ActionTypes.MessageBack, "message back local echo", null, "text received by bots", "display text message back", JObject.Parse(@"{ ""key"" : ""value"" }"));
            var action4 = new CardAction("invoke", "invoke", null, null, null, JObject.Parse(@"{ ""key"" : ""value"" }"));

            adaptiveCard.Actions.Add(action1.ToAdaptiveCardAction());
            adaptiveCard.Actions.Add(action2.ToAdaptiveCardAction());
            adaptiveCard.Actions.Add(action3.ToAdaptiveCardAction());
            adaptiveCard.Actions.Add(action4.ToAdaptiveCardAction());

            var replyActivity = MessageFactory.Attachment(adaptiveCard.ToAttachment());
            await turnContext.SendActivityAsync(replyActivity, cancellationToken);
        }

        private async Task HandleDeleteActivitiesAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var activityId in _activityIds)
            {
                await turnContext.DeleteActivityAsync(activityId, cancellationToken);
            }

            this._activityIds.Clear();
        }

        private async Task HandleUpdateActivitiesAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var activityId in _activityIds)
            {
                var newActivity = MessageFactory.Text(turnContext.Activity.Text);
                newActivity.Id = activityId;
                await turnContext.UpdateActivityAsync(newActivity, cancellationToken);
            }
        }

        private async Task SendMessageAndLogActivityIdAsync(ITurnContext turnContext, string text, CancellationToken cancellationToken)
        {
            // We need to record the Activity Id from the Activity just sent in order to understand what the reaction is a reaction too. 
            var replyActivity = MessageFactory.Text($"You said '{text}'");
            var resourceResponse = await turnContext.SendActivityAsync(replyActivity, cancellationToken);
            _activityIds.Add(resourceResponse.Id);
            await _log.Append(resourceResponse.Id, replyActivity);
        }
    }
}
