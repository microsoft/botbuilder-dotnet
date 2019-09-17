// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Tests;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Teams.Tests
{
    [TestClass]
    public class TeamsActivityHandlerTests
    {
        [TestMethod]
        public async Task TestConversationUpdateTeamsMemberAdded()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.ConversationUpdate,
                MembersAdded = new List<ChannelAccount>
                {
                    new ChannelAccount { Id = "a" },
                },
                Recipient = new ChannelAccount { Id = "b" },
                ChannelData = new TeamsChannelData { EventType = "teamMemberAdded" },
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

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
                MembersAdded = new List<ChannelAccount>
                {
                    new ChannelAccount { Id = "a" },
                },
                Recipient = new ChannelAccount { Id = "b" },
                ChannelData = new TeamsChannelData { EventType = "teamMemberRemoved" },
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
        public async Task TestInvoke()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "gibberish",
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(1, bot.Record.Count);
            Assert.AreEqual("OnInvokeActivityAsync", bot.Record[0]);
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

            protected override Task OnTeamsMembersAddedAsync(IList<ChannelAccount> membersAdded, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return Task.CompletedTask;
            }

            protected override Task OnTeamsMembersRemovedAsync(IList<ChannelAccount> membersRemoved, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
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
                return base.OnTeamsFileConsentAcceptAsync(turnContext, fileConsentCardResponse, cancellationToken);
            }

            protected override Task OnTeamsFileConsentDeclineAsync(ITurnContext<IInvokeActivity> turnContext, FileConsentCardResponse fileConsentCardResponse, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnTeamsFileConsentDeclineAsync(turnContext, fileConsentCardResponse, cancellationToken);
            }

            protected override Task OnTeamsO365ConnectorCardActionAsync(ITurnContext<IInvokeActivity> turnContext, O365ConnectorCardActionQuery query, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnTeamsO365ConnectorCardActionAsync(turnContext, query, cancellationToken);
            }

            protected override Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionBotMessagePreviewEditAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction query, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnTeamsMessagingExtensionBotMessagePreviewEditAsync(turnContext, query, cancellationToken);
            }

            protected override Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionBotMessagePreviewSendAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction query, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnTeamsMessagingExtensionBotMessagePreviewSendAsync(turnContext, query, cancellationToken);
            }

            protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionConfigurationSettingsAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnTeamsMessagingExtensionConfigurationSettingsAsync(turnContext, cancellationToken);
            }

            protected override Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnTeamsMessagingExtensionFetchTaskAsync(turnContext, cancellationToken);
            }

            protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionConfigurationSettingsUrlAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnTeamsMessagingExtensionConfigurationSettingsUrlAsync(turnContext, cancellationToken);
            }

            protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnTeamsMessagingExtensionQueryAsync(turnContext, query, cancellationToken);
            }

            protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionSelectItemAsync(ITurnContext<IInvokeActivity> turnContext, JObject query, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnTeamsMessagingExtensionSelectItemAsync(turnContext, query, cancellationToken);
            }

            protected override Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction query, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnTeamsMessagingExtensionSubmitActionAsync(turnContext, query, cancellationToken);
            }

            protected override Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionDispatchAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction query, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnTeamsMessagingExtensionSubmitActionDispatchAsync(turnContext, query, cancellationToken);
            }

            protected override Task<MessagingExtensionResponse> OnTeamsAppBasedLinkQueryAsync(ITurnContext<IInvokeActivity> turnContext, AppBasedLinkQuery query, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnTeamsAppBasedLinkQueryAsync(turnContext, query, cancellationToken);
            }

            protected override Task<InvokeResponse> OnTeamsCardActionInvokeAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnTeamsCardActionInvokeAsync(turnContext, cancellationToken);
            }

            protected override Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnTeamsTaskModuleFetchAsync(turnContext, cancellationToken);
            }

            protected override Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnTeamsTaskModuleSubmitAsync(turnContext, cancellationToken);
            }
        }
    }
}
