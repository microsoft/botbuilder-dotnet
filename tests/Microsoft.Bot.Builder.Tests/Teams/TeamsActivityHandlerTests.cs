// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Tests;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Rest.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Teams.Tests
{
    [TestClass]
    public class TeamsActivityHandlerTests
    {
        [TestMethod]
        public async Task TestConversationUpdateTeamsMemberAdded()
        {
            // Arrange
            var baseUri = new Uri("https://test.coffee");
            var customHttpClient = new HttpClient(new RosterHttpMessageHandler());

            // Set a special base address so then we can make sure the connector client is honoring this http client
            customHttpClient.BaseAddress = baseUri;
            var connectorClient = new ConnectorClient(new Uri("http://localhost/"), new MicrosoftAppCredentials(string.Empty, string.Empty), customHttpClient);

            var activity = new Activity
            {
                Type = ActivityTypes.ConversationUpdate,
                MembersAdded = new List<ChannelAccount>
                {
                    new ChannelAccount { Id = "id-1" },
                },
                Recipient = new ChannelAccount { Id = "b" },
                ChannelData = new TeamsChannelData
                {
                    EventType = "teamMemberAdded",
                    Team = new TeamInfo
                    {
                        Id = "team-id",
                    },
                },
                ChannelId = Channels.Msteams,
            };

            var turnContext = new TurnContext(new SimpleAdapter(), activity);
            turnContext.TurnState.Add<IConnectorClient>(connectorClient);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual("OnConversationUpdateActivityAsync", bot.Record[0]);
            Assert.AreEqual("OnTeamsMembersAddedAsync", bot.Record[1]);
        }

        [TestMethod]
        public async Task TestConversationUpdateTeamsMemberAddedNoTeam()
        {
            // Arrange
            var baseUri = new Uri("https://test.coffee");
            var customHttpClient = new HttpClient(new RosterHttpMessageHandler());

            // Set a special base address so then we can make sure the connector client is honoring this http client
            customHttpClient.BaseAddress = baseUri;
            var connectorClient = new ConnectorClient(new Uri("http://localhost/"), new MicrosoftAppCredentials(string.Empty, string.Empty), customHttpClient);

            var activity = new Activity
            {
                Type = ActivityTypes.ConversationUpdate,
                MembersAdded = new List<ChannelAccount>
                {
                    new ChannelAccount { Id = "id-1" },
                },
                Recipient = new ChannelAccount { Id = "b" },
                Conversation = new ConversationAccount { Id = "conversation-id" },
                ChannelId = Channels.Msteams,
            };

            var turnContext = new TurnContext(new SimpleAdapter(), activity);
            turnContext.TurnState.Add<IConnectorClient>(connectorClient);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual("OnConversationUpdateActivityAsync", bot.Record[0]);
            Assert.AreEqual("OnTeamsMembersAddedAsync", bot.Record[1]);
        }

        [TestMethod]
        public async Task TestConversationUpdateTeamsMemberAddedFullDetailsInEvent()
        {
            // Arrange
            var baseUri = new Uri("https://test.coffee");
            var customHttpClient = new HttpClient(new RosterHttpMessageHandler());

            // Set a special base address so then we can make sure the connector client is honoring this http client
            customHttpClient.BaseAddress = baseUri;
            var connectorClient = new ConnectorClient(new Uri("http://localhost/"), new MicrosoftAppCredentials(string.Empty, string.Empty), customHttpClient);

            var activity = new Activity
            {
                Type = ActivityTypes.ConversationUpdate,
                MembersAdded = new List<ChannelAccount>
                {
                    new TeamsChannelAccount
                    {
                        Id = "id-1",
                        Name = "name-1",
                        AadObjectId = "aadobject-1",
                        Email = "test@microsoft.com",
                        GivenName = "given-1",
                        Surname = "surname-1",
                        UserPrincipalName = "t@microsoft.com",
                    },
                },
                Recipient = new ChannelAccount { Id = "b" },
                ChannelData = new TeamsChannelData
                {
                    EventType = "teamMemberAdded",
                    Team = new TeamInfo
                    {
                        Id = "team-id",
                    },
                },
                ChannelId = Channels.Msteams,
            };

            // code taken from connector - i.e. the send or serialize side
            var serializationSettings = new JsonSerializerSettings();
            serializationSettings.ContractResolver = new DefaultContractResolver();
            var json = Rest.Serialization.SafeJsonConvert.SerializeObject(activity, serializationSettings);

            // code taken from integration layer - i.e. the receive or deserialize side
            var botMessageSerializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                ContractResolver = new ReadOnlyJsonContractResolver(),
                Converters = new List<JsonConverter> { new Iso8601TimeSpanConverter() },
            });

            using (var bodyReader = new JsonTextReader(new StringReader(json)))
            {
                activity = botMessageSerializer.Deserialize<Activity>(bodyReader);
            }

            var turnContext = new TurnContext(new SimpleAdapter(), activity);
            turnContext.TurnState.Add<IConnectorClient>(connectorClient);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual("OnConversationUpdateActivityAsync", bot.Record[0]);
            Assert.AreEqual("OnTeamsMembersAddedAsync", bot.Record[1]);
        }

        [TestMethod]
        public async Task TestConversationUpdateTeamsMemberRemoved()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.ConversationUpdate,
                MembersRemoved = new List<ChannelAccount>
                {
                    new ChannelAccount { Id = "a" },
                },
                Recipient = new ChannelAccount { Id = "b" },
                ChannelData = new TeamsChannelData { EventType = "teamMemberRemoved" },
                ChannelId = Channels.Msteams,
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual("OnConversationUpdateActivityAsync", bot.Record[0]);
            Assert.AreEqual("OnTeamsMembersRemovedAsync", bot.Record[1]);
        }

        [TestMethod]
        public async Task TestConversationUpdateTeamsChannelCreated()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.ConversationUpdate,
                ChannelData = new TeamsChannelData { EventType = "channelCreated" },
                ChannelId = Channels.Msteams,
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual("OnConversationUpdateActivityAsync", bot.Record[0]);
            Assert.AreEqual("OnTeamsChannelCreatedAsync", bot.Record[1]);
        }

        [TestMethod]
        public async Task TestConversationUpdateTeamsChannelDeleted()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.ConversationUpdate,
                ChannelData = new TeamsChannelData { EventType = "channelDeleted" },
                ChannelId = Channels.Msteams,
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual("OnConversationUpdateActivityAsync", bot.Record[0]);
            Assert.AreEqual("OnTeamsChannelDeletedAsync", bot.Record[1]);
        }

        [TestMethod]
        public async Task TestConversationUpdateTeamsChannelRenamed()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.ConversationUpdate,
                ChannelData = new TeamsChannelData { EventType = "channelRenamed" },
                ChannelId = Channels.Msteams,
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual("OnConversationUpdateActivityAsync", bot.Record[0]);
            Assert.AreEqual("OnTeamsChannelRenamedAsync", bot.Record[1]);
        }

        [TestMethod]
        public async Task TestConversationUpdateTeamsTeamRenamed()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.ConversationUpdate,
                ChannelData = new TeamsChannelData { EventType = "teamRenamed" },
                ChannelId = Channels.Msteams,
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual("OnConversationUpdateActivityAsync", bot.Record[0]);
            Assert.AreEqual("OnTeamsTeamRenamedAsync", bot.Record[1]);
        }

        [TestMethod]
        public async Task TestFileConsentAccept()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "fileConsent/invoke",
                Value = JObject.FromObject(new FileConsentCardResponse
                {
                    Action = "accept",
                    UploadInfo = new FileUploadInfo
                    {
                        UniqueId = "uniqueId",
                        FileType = "fileType",
                        UploadUrl = "uploadUrl",
                    },
                }),
            };

            Activity[] activitiesToSend = null;
            void CaptureSend(Activity[] arg)
            {
                activitiesToSend = arg;
            }

            var turnContext = new TurnContext(new SimpleAdapter(CaptureSend), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(3, bot.Record.Count);
            Assert.AreEqual("OnInvokeActivityAsync", bot.Record[0]);
            Assert.AreEqual("OnTeamsFileConsentAsync", bot.Record[1]);
            Assert.AreEqual("OnTeamsFileConsentAcceptAsync", bot.Record[2]);
            Assert.IsNotNull(activitiesToSend);
            Assert.AreEqual(1, activitiesToSend.Length);
            Assert.IsInstanceOfType(activitiesToSend[0].Value, typeof(InvokeResponse));
            Assert.AreEqual(200, ((InvokeResponse)activitiesToSend[0].Value).Status);
        }

        [TestMethod]
        public async Task TestFileConsentDecline()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "fileConsent/invoke",
                Value = JObject.FromObject(new FileConsentCardResponse
                {
                    Action = "decline",
                    UploadInfo = new FileUploadInfo
                    {
                        UniqueId = "uniqueId",
                        FileType = "fileType",
                        UploadUrl = "uploadUrl",
                    },
                }),
            };

            Activity[] activitiesToSend = null;
            void CaptureSend(Activity[] arg)
            {
                activitiesToSend = arg;
            }

            var turnContext = new TurnContext(new SimpleAdapter(CaptureSend), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(3, bot.Record.Count);
            Assert.AreEqual("OnInvokeActivityAsync", bot.Record[0]);
            Assert.AreEqual("OnTeamsFileConsentAsync", bot.Record[1]);
            Assert.AreEqual("OnTeamsFileConsentDeclineAsync", bot.Record[2]);
            Assert.IsNotNull(activitiesToSend);
            Assert.AreEqual(1, activitiesToSend.Length);
            Assert.IsInstanceOfType(activitiesToSend[0].Value, typeof(InvokeResponse));
            Assert.AreEqual(200, ((InvokeResponse)activitiesToSend[0].Value).Status);
        }

        [TestMethod]
        public async Task TestActionableMessageExecuteAction()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "actionableMessage/executeAction",
                Value = JObject.FromObject(new O365ConnectorCardActionQuery()),
            };

            Activity[] activitiesToSend = null;
            void CaptureSend(Activity[] arg)
            {
                activitiesToSend = arg;
            }

            var turnContext = new TurnContext(new SimpleAdapter(CaptureSend), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual("OnInvokeActivityAsync", bot.Record[0]);
            Assert.AreEqual("OnTeamsO365ConnectorCardActionAsync", bot.Record[1]);
            Assert.IsNotNull(activitiesToSend);
            Assert.AreEqual(1, activitiesToSend.Length);
            Assert.IsInstanceOfType(activitiesToSend[0].Value, typeof(InvokeResponse));
            Assert.AreEqual(200, ((InvokeResponse)activitiesToSend[0].Value).Status);
        }

        [TestMethod]
        public async Task TestComposeExtensionQueryLink()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "composeExtension/queryLink",
                Value = JObject.FromObject(new AppBasedLinkQuery()),
            };

            Activity[] activitiesToSend = null;
            void CaptureSend(Activity[] arg)
            {
                activitiesToSend = arg;
            }

            var turnContext = new TurnContext(new SimpleAdapter(CaptureSend), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual("OnInvokeActivityAsync", bot.Record[0]);
            Assert.AreEqual("OnTeamsAppBasedLinkQueryAsync", bot.Record[1]);
            Assert.IsNotNull(activitiesToSend);
            Assert.AreEqual(1, activitiesToSend.Length);
            Assert.IsInstanceOfType(activitiesToSend[0].Value, typeof(InvokeResponse));
            Assert.AreEqual(200, ((InvokeResponse)activitiesToSend[0].Value).Status);
        }

        [TestMethod]
        public async Task TestComposeExtensionQuery()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "composeExtension/query",
                Value = JObject.FromObject(new MessagingExtensionQuery()),
            };

            Activity[] activitiesToSend = null;
            void CaptureSend(Activity[] arg)
            {
                activitiesToSend = arg;
            }

            var turnContext = new TurnContext(new SimpleAdapter(CaptureSend), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual("OnInvokeActivityAsync", bot.Record[0]);
            Assert.AreEqual("OnTeamsMessagingExtensionQueryAsync", bot.Record[1]);
            Assert.IsNotNull(activitiesToSend);
            Assert.AreEqual(1, activitiesToSend.Length);
            Assert.IsInstanceOfType(activitiesToSend[0].Value, typeof(InvokeResponse));
            Assert.AreEqual(200, ((InvokeResponse)activitiesToSend[0].Value).Status);
        }

        [TestMethod]
        public async Task TestMessagingExtensionSelectItemAsync()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "composeExtension/selectItem",
                Value = new JObject(),
            };

            Activity[] activitiesToSend = null;
            void CaptureSend(Activity[] arg)
            {
                activitiesToSend = arg;
            }

            var turnContext = new TurnContext(new SimpleAdapter(CaptureSend), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual("OnInvokeActivityAsync", bot.Record[0]);
            Assert.AreEqual("OnTeamsMessagingExtensionSelectItemAsync", bot.Record[1]);
            Assert.IsNotNull(activitiesToSend);
            Assert.AreEqual(1, activitiesToSend.Length);
            Assert.IsInstanceOfType(activitiesToSend[0].Value, typeof(InvokeResponse));
            Assert.AreEqual(200, ((InvokeResponse)activitiesToSend[0].Value).Status);
        }

        [TestMethod]
        public async Task TestMessagingExtensionSubmitAction()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "composeExtension/submitAction",
                Value = JObject.FromObject(new MessagingExtensionQuery()),
            };

            Activity[] activitiesToSend = null;
            void CaptureSend(Activity[] arg)
            {
                activitiesToSend = arg;
            }

            var turnContext = new TurnContext(new SimpleAdapter(CaptureSend), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(3, bot.Record.Count);
            Assert.AreEqual("OnInvokeActivityAsync", bot.Record[0]);
            Assert.AreEqual("OnTeamsMessagingExtensionSubmitActionDispatchAsync", bot.Record[1]);
            Assert.AreEqual("OnTeamsMessagingExtensionSubmitActionAsync", bot.Record[2]);
            Assert.IsNotNull(activitiesToSend);
            Assert.AreEqual(1, activitiesToSend.Length);
            Assert.IsInstanceOfType(activitiesToSend[0].Value, typeof(InvokeResponse));
            Assert.AreEqual(200, ((InvokeResponse)activitiesToSend[0].Value).Status);
        }

        [TestMethod]
        public async Task TestMessagingExtensionSubmitActionPreviewActionEdit()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "composeExtension/submitAction",
                Value = JObject.FromObject(new MessagingExtensionAction
                {
                    BotMessagePreviewAction = "edit",
                }),
            };

            Activity[] activitiesToSend = null;
            void CaptureSend(Activity[] arg)
            {
                activitiesToSend = arg;
            }

            var turnContext = new TurnContext(new SimpleAdapter(CaptureSend), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(3, bot.Record.Count);
            Assert.AreEqual("OnInvokeActivityAsync", bot.Record[0]);
            Assert.AreEqual("OnTeamsMessagingExtensionSubmitActionDispatchAsync", bot.Record[1]);
            Assert.AreEqual("OnTeamsMessagingExtensionBotMessagePreviewEditAsync", bot.Record[2]);
            Assert.IsNotNull(activitiesToSend);
            Assert.AreEqual(1, activitiesToSend.Length);
            Assert.IsInstanceOfType(activitiesToSend[0].Value, typeof(InvokeResponse));
            Assert.AreEqual(200, ((InvokeResponse)activitiesToSend[0].Value).Status);
        }

        [TestMethod]
        public async Task TestMessagingExtensionSubmitActionPreviewActionSend()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "composeExtension/submitAction",
                Value = JObject.FromObject(new MessagingExtensionAction
                {
                    BotMessagePreviewAction = "send",
                }),
            };

            Activity[] activitiesToSend = null;
            void CaptureSend(Activity[] arg)
            {
                activitiesToSend = arg;
            }

            var turnContext = new TurnContext(new SimpleAdapter(CaptureSend), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(3, bot.Record.Count);
            Assert.AreEqual("OnInvokeActivityAsync", bot.Record[0]);
            Assert.AreEqual("OnTeamsMessagingExtensionSubmitActionDispatchAsync", bot.Record[1]);
            Assert.AreEqual("OnTeamsMessagingExtensionBotMessagePreviewSendAsync", bot.Record[2]);
            Assert.IsNotNull(activitiesToSend);
            Assert.AreEqual(1, activitiesToSend.Length);
            Assert.IsInstanceOfType(activitiesToSend[0].Value, typeof(InvokeResponse));
            Assert.AreEqual(200, ((InvokeResponse)activitiesToSend[0].Value).Status);
        }

        [TestMethod]
        public async Task TestMessagingExtensionFetchTask()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "composeExtension/fetchTask",
                Value = JObject.Parse(@"{""commandId"":""testCommand""}"),
            };

            Activity[] activitiesToSend = null;
            void CaptureSend(Activity[] arg)
            {
                activitiesToSend = arg;
            }

            var turnContext = new TurnContext(new SimpleAdapter(CaptureSend), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual("OnInvokeActivityAsync", bot.Record[0]);
            Assert.AreEqual("OnTeamsMessagingExtensionFetchTaskAsync", bot.Record[1]);
            Assert.IsNotNull(activitiesToSend);
            Assert.AreEqual(1, activitiesToSend.Length);
            Assert.IsInstanceOfType(activitiesToSend[0].Value, typeof(InvokeResponse));
            Assert.AreEqual(200, ((InvokeResponse)activitiesToSend[0].Value).Status);
        }

        [TestMethod]
        public async Task TestMessagingExtensionConfigurationQuerySettingUrl()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "composeExtension/querySettingUrl",
                Value = JObject.Parse(@"{""commandId"":""testCommand""}"),
            };

            Activity[] activitiesToSend = null;
            void CaptureSend(Activity[] arg)
            {
                activitiesToSend = arg;
            }

            var turnContext = new TurnContext(new SimpleAdapter(CaptureSend), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual("OnInvokeActivityAsync", bot.Record[0]);
            Assert.AreEqual("OnTeamsMessagingExtensionConfigurationQuerySettingUrlAsync", bot.Record[1]);
            Assert.IsNotNull(activitiesToSend);
            Assert.AreEqual(1, activitiesToSend.Length);
            Assert.IsInstanceOfType(activitiesToSend[0].Value, typeof(InvokeResponse));
            Assert.AreEqual(200, ((InvokeResponse)activitiesToSend[0].Value).Status);
        }

        [TestMethod]
        public async Task TestMessagingExtensionConfigurationSetting()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "composeExtension/setting",
                Value = JObject.Parse(@"{""commandId"":""testCommand""}"),
            };

            Activity[] activitiesToSend = null;
            void CaptureSend(Activity[] arg)
            {
                activitiesToSend = arg;
            }

            var turnContext = new TurnContext(new SimpleAdapter(CaptureSend), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual("OnInvokeActivityAsync", bot.Record[0]);
            Assert.AreEqual("OnTeamsMessagingExtensionConfigurationSettingAsync", bot.Record[1]);
            Assert.IsNotNull(activitiesToSend);
            Assert.AreEqual(1, activitiesToSend.Length);
            Assert.IsInstanceOfType(activitiesToSend[0].Value, typeof(InvokeResponse));
            Assert.AreEqual(200, ((InvokeResponse)activitiesToSend[0].Value).Status);
        }

        [TestMethod]
        public async Task TestTaskModuleFetch()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "task/fetch",
                Value = JObject.Parse(@"{""data"":{""key"":""value"",""type"":""task / fetch""},""context"":{""theme"":""default""}}"),
            };

            Activity[] activitiesToSend = null;
            void CaptureSend(Activity[] arg)
            {
                activitiesToSend = arg;
            }

            var turnContext = new TurnContext(new SimpleAdapter(CaptureSend), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual("OnInvokeActivityAsync", bot.Record[0]);
            Assert.AreEqual("OnTeamsTaskModuleFetchAsync", bot.Record[1]);
            Assert.IsNotNull(activitiesToSend);
            Assert.AreEqual(1, activitiesToSend.Length);
            Assert.IsInstanceOfType(activitiesToSend[0].Value, typeof(InvokeResponse));
            Assert.AreEqual(200, ((InvokeResponse)activitiesToSend[0].Value).Status);
        }

        [TestMethod]
        public async Task TestTaskModuleSubmit()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "task/submit",
                Value = JObject.Parse(@"{""data"":{""key"":""value"",""type"":""task / fetch""},""context"":{""theme"":""default""}}"),
            };

            Activity[] activitiesToSend = null;
            void CaptureSend(Activity[] arg)
            {
                activitiesToSend = arg;
            }

            var turnContext = new TurnContext(new SimpleAdapter(CaptureSend), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual("OnInvokeActivityAsync", bot.Record[0]);
            Assert.AreEqual("OnTeamsTaskModuleSubmitAsync", bot.Record[1]);
            Assert.IsNotNull(activitiesToSend);
            Assert.AreEqual(1, activitiesToSend.Length);
            Assert.IsInstanceOfType(activitiesToSend[0].Value, typeof(InvokeResponse));
            Assert.AreEqual(200, ((InvokeResponse)activitiesToSend[0].Value).Status);
        }

        [TestMethod]
        public async Task TestSigninVerifyState()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "signin/verifyState",
            };

            Activity[] activitiesToSend = null;
            void CaptureSend(Activity[] arg)
            {
                activitiesToSend = arg;
            }

            var turnContext = new TurnContext(new SimpleAdapter(CaptureSend), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);
           
            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual("OnInvokeActivityAsync", bot.Record[0]);
            Assert.AreEqual("OnTeamsSigninVerifyStateAsync", bot.Record[1]);
            Assert.IsNotNull(activitiesToSend);
            Assert.AreEqual(1, activitiesToSend.Length);
            Assert.IsInstanceOfType(activitiesToSend[0].Value, typeof(InvokeResponse));
            Assert.AreEqual(200, ((InvokeResponse)activitiesToSend[0].Value).Status);
        }

        private class NotImplementedAdapter : BotAdapter
        {
            public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public override Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        private class TestActivityHandler : TeamsActivityHandler
        {
            public List<string> Record { get; } = new List<string>();

            // ConversationUpdate
            protected override Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
            }

            protected override Task OnTeamsChannelCreatedAsync(ChannelInfo channelInfo, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnTeamsChannelCreatedAsync(channelInfo, teamInfo, turnContext, cancellationToken);
            }

            protected override Task OnTeamsChannelDeletedAsync(ChannelInfo channelInfo, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnTeamsChannelDeletedAsync(channelInfo, teamInfo, turnContext, cancellationToken);
            }

            protected override Task OnTeamsChannelRenamedAsync(ChannelInfo channelInfo, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnTeamsChannelRenamedAsync(channelInfo, teamInfo, turnContext, cancellationToken);
            }

            protected override Task OnTeamsTeamRenamedAsync(TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnTeamsTeamRenamedAsync(teamInfo, turnContext, cancellationToken);
            }

            protected override Task OnTeamsMembersAddedAsync(IList<TeamsChannelAccount> membersAdded, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return Task.CompletedTask;
            }

            protected override Task OnTeamsMembersRemovedAsync(IList<TeamsChannelAccount> membersRemoved, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return Task.CompletedTask;
            }

            protected override Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return Task.CompletedTask;
            }

            protected override Task OnMembersRemovedAsync(IList<ChannelAccount> membersRemoved, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return Task.CompletedTask;
            }

            // Invoke
            protected override Task<InvokeResponse> OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnInvokeActivityAsync(turnContext, cancellationToken);
            }

            protected override Task<InvokeResponse> OnTeamsFileConsentAsync(ITurnContext<IInvokeActivity> turnContext, FileConsentCardResponse fileConsentCardResponse, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnTeamsFileConsentAsync(turnContext, fileConsentCardResponse, cancellationToken);
            }

            protected override Task OnTeamsFileConsentAcceptAsync(ITurnContext<IInvokeActivity> turnContext, FileConsentCardResponse fileConsentCardResponse, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return Task.CompletedTask;
            }

            protected override Task OnTeamsFileConsentDeclineAsync(ITurnContext<IInvokeActivity> turnContext, FileConsentCardResponse fileConsentCardResponse, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return Task.CompletedTask;
            }

            protected override Task OnTeamsO365ConnectorCardActionAsync(ITurnContext<IInvokeActivity> turnContext, O365ConnectorCardActionQuery query, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return Task.CompletedTask;
            }

            protected override Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionBotMessagePreviewEditAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return Task.FromResult(new MessagingExtensionActionResponse());
            }

            protected override Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionBotMessagePreviewSendAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return Task.FromResult(new MessagingExtensionActionResponse());
            }

            protected override Task OnTeamsMessagingExtensionCardButtonClickedAsync(ITurnContext<IInvokeActivity> turnContext, JObject obj, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnTeamsMessagingExtensionCardButtonClickedAsync(turnContext, obj, cancellationToken);
            }

            protected override Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return Task.FromResult(new MessagingExtensionActionResponse());
            }

            protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionConfigurationQuerySettingUrlAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return Task.FromResult(new MessagingExtensionResponse());
            }

            protected override Task OnTeamsMessagingExtensionConfigurationSettingAsync(ITurnContext<IInvokeActivity> turnContext, JObject obj, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return Task.CompletedTask;
            }

            protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return Task.FromResult(new MessagingExtensionResponse());
            }

            protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionSelectItemAsync(ITurnContext<IInvokeActivity> turnContext, JObject query, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return Task.FromResult(new MessagingExtensionResponse());
            }

            protected override Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return Task.FromResult(new MessagingExtensionActionResponse());
            }

            protected override Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionDispatchAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnTeamsMessagingExtensionSubmitActionDispatchAsync(turnContext, action, cancellationToken);
            }

            protected override Task<MessagingExtensionResponse> OnTeamsAppBasedLinkQueryAsync(ITurnContext<IInvokeActivity> turnContext, AppBasedLinkQuery query, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return Task.FromResult(new MessagingExtensionResponse());
            }

            protected override Task<InvokeResponse> OnTeamsCardActionInvokeAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnTeamsCardActionInvokeAsync(turnContext, cancellationToken);
            }

            protected override Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return Task.FromResult(new TaskModuleResponse());
            }

            protected override Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return Task.FromResult(new TaskModuleResponse());
            }

            protected override Task OnTeamsSigninVerifyStateAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return Task.CompletedTask;
            }
        }

        private class RosterHttpMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                
                // GetMembers (Team)
                if (request.RequestUri.PathAndQuery.EndsWith("team-id/members"))
                {
                    var content = new JArray
                    {
                        new JObject
                        {
                            new JProperty("id", "id-1"),
                            new JProperty("objectId", "objectId-1"),
                            new JProperty("name", "name-1"),
                            new JProperty("givenName", "givenName-1"),
                            new JProperty("surname", "surname-1"),
                            new JProperty("email", "email-1"),
                            new JProperty("userPrincipalName", "userPrincipalName-1"),
                            new JProperty("tenantId", "tenantId-1"),
                        },
                        new JObject
                        {
                            new JProperty("id", "id-2"),
                            new JProperty("objectId", "objectId-2"),
                            new JProperty("name", "name-2"),
                            new JProperty("givenName", "givenName-2"),
                            new JProperty("surname", "surname-2"),
                            new JProperty("email", "email-2"),
                            new JProperty("userPrincipalName", "userPrincipalName-2"),
                            new JProperty("tenantId", "tenantId-2"),
                        },
                    };
                    response.Content = new StringContent(content.ToString());
                }

                // GetMembers (Group Chat)
                else if (request.RequestUri.PathAndQuery.EndsWith("conversation-id/members"))
                {
                    var content = new JArray
                    {
                        new JObject
                        {
                            new JProperty("id", "id-3"),
                            new JProperty("objectId", "objectId-3"),
                            new JProperty("name", "name-3"),
                            new JProperty("givenName", "givenName-3"),
                            new JProperty("surname", "surname-3"),
                            new JProperty("email", "email-3"),
                            new JProperty("userPrincipalName", "userPrincipalName-3"),
                            new JProperty("tenantId", "tenantId-3"),
                        },
                        new JObject
                        {
                            new JProperty("id", "id-4"),
                            new JProperty("objectId", "objectId-4"),
                            new JProperty("name", "name-4"),
                            new JProperty("givenName", "givenName-4"),
                            new JProperty("surname", "surname-4"),
                            new JProperty("email", "email-4"),
                            new JProperty("userPrincipalName", "userPrincipalName-4"),
                            new JProperty("tenantId", "tenantId-4"),
                        },
                    };
                    response.Content = new StringContent(content.ToString());
                }
                else if (request.RequestUri.PathAndQuery.EndsWith("team-id/members/id-1") || request.RequestUri.PathAndQuery.EndsWith("conversation-id/members/id-1"))
                {
                    var content = new JObject
                        {
                            new JProperty("id", "id-1"),
                            new JProperty("objectId", "objectId-1"),
                            new JProperty("name", "name-1"),
                            new JProperty("givenName", "givenName-1"),
                            new JProperty("surname", "surname-1"),
                            new JProperty("email", "email-1"),
                            new JProperty("userPrincipalName", "userPrincipalName-1"),
                            new JProperty("tenantId", "tenantId-1"),
                        };
                    response.Content = new StringContent(content.ToString());
                }

                return Task.FromResult(response);
            }
        }
    }
}
