// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public class ActivityHandlerTests
    {
        [TestMethod]
        public async Task TestMessageActivity()
        {
            // Arrange
            var activity = MessageFactory.Text("hello");
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(1, bot.Record.Count);
            Assert.AreEqual(bot.Record[0], "OnMessageActivityAsync");
        }

        [TestMethod]
        public async Task TestMemberAdded()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.ConversationUpdate,
                MembersAdded = new List<ChannelAccount>
                {
                    new ChannelAccount { Id = "a" },
                    new ChannelAccount { Id = "b" },
                    new ChannelAccount { Id = "c" },
                },
                Recipient = new ChannelAccount {  Id = "b" }
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual(bot.Record[0], "OnConversationUpdateActivityAsync");
            Assert.AreEqual(bot.Record[1], "OnMembersAddedAsync");
        }

        [TestMethod]
        public async Task TestMemberRemoved()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.ConversationUpdate,
                MembersAdded = new List<ChannelAccount>
                {
                    new ChannelAccount { Id = "a" },
                    new ChannelAccount { Id = "b" },
                    new ChannelAccount { Id = "c" },
                },
                Recipient = new ChannelAccount { Id = "c" }
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual(bot.Record[0], "OnConversationUpdateActivityAsync");
            Assert.AreEqual(bot.Record[1], "OnMembersAddedAsync");
        }

        [TestMethod]
        public async Task TestMemberAddedJustTheBot()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.ConversationUpdate,
                MembersAdded = new List<ChannelAccount>
                {
                    new ChannelAccount { Id = "b" },
                },
                Recipient = new ChannelAccount { Id = "b" }
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(1, bot.Record.Count);
            Assert.AreEqual(bot.Record[0], "OnConversationUpdateActivityAsync");
        }

        [TestMethod]
        public async Task TestMemberRemovedJustTheBot()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.ConversationUpdate,
                MembersAdded = new List<ChannelAccount>
                {
                    new ChannelAccount { Id = "c" },
                },
                Recipient = new ChannelAccount { Id = "c" }
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(1, bot.Record.Count);
            Assert.AreEqual(bot.Record[0], "OnConversationUpdateActivityAsync");
        }

        [TestMethod]
        public async Task TestTokenResponseEventAsync()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Event,
                Name = "tokens/response",
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual(bot.Record[0], "OnEventActivityAsync");
            Assert.AreEqual(bot.Record[1], "OnTokenResponseEventAsync");
        }

        [TestMethod]
        public async Task TestCreateConversationAsync()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Event,
                Name = "createConversation",
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual(bot.Record[0], "OnEventActivityAsync");
            Assert.AreEqual(bot.Record[1], "OnCreateConversationAsync");
        }

        [TestMethod]
        public async Task TestContinueConversationAsync()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Event,
                Name = "continueConversation",
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual(bot.Record[0], "OnEventActivityAsync");
            Assert.AreEqual(bot.Record[1], "OnContinueConversationAsync");
        }

        [TestMethod]
        public async Task TestEventAsync()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Event,
                Name = "some.random.event",
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual(bot.Record[0], "OnEventActivityAsync");
            Assert.AreEqual(bot.Record[1], "OnEventAsync");
        }

        [TestMethod]
        public async Task TestEventNullNameAsync()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Event
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual(bot.Record[0], "OnEventActivityAsync");
            Assert.AreEqual(bot.Record[1], "OnEventAsync");
        }

        [TestMethod]
        public async Task TestContactRelationUpdateActivity()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.ContactRelationUpdate
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(1, bot.Record.Count);
            Assert.AreEqual(bot.Record[0], "OnContactRelationUpdateActivityAsync");
        }

        [TestMethod]
        public async Task TestTeamsVerificationInvokeAsync()
        {
            // Arrange
            var adapter = new TestInvokeAdapter();
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "signin/verifyState",
            };
            var turnContext = new TurnContext(adapter, activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual(bot.Record[0], "OnInvokeActivityAsync");
            Assert.AreEqual(bot.Record[1], "OnTeamsVerificationInvokeAsync");
            Assert.IsNotNull(adapter.Activity);
            Assert.AreEqual((int)HttpStatusCode.OK, ((InvokeResponse)((Activity)adapter.Activity).Value).Status);
        }

        [TestMethod]
        public async Task TestInvokeActivity()
        {
            // Arrange
            var adapter = new TestInvokeAdapter();
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke
            };
            var turnContext = new TurnContext(adapter, activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual(bot.Record[0], "OnInvokeActivityAsync");
            Assert.AreEqual(bot.Record[1], "OnInvokeAsync");
            Assert.IsNotNull(adapter.Activity);
            Assert.AreEqual((int)HttpStatusCode.OK, ((InvokeResponse)((Activity)adapter.Activity).Value).Status);
        }

        [TestMethod]
        public async Task TestEndOfConversationActivity()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.EndOfConversation
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(1, bot.Record.Count);
            Assert.AreEqual(bot.Record[0], "OnEndOfConversationActivityAsync");
        }

        [TestMethod]
        public async Task TestDeleteUserDataActivity()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.DeleteUserData
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(1, bot.Record.Count);
            Assert.AreEqual(bot.Record[0], "OnDeleteUserDataActivityAsync");
        }

        [TestMethod]
        public async Task TestMessageReactionActivityAdded()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.MessageReaction,
                ReactionsAdded = new List<MessageReaction> { new MessageReaction { Type = "x" } },
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual(bot.Record[0], "OnMessageReactionActivityAsync");
            Assert.AreEqual(bot.Record[1], "OnMessageReactionsAddedAsync");
        }

        [TestMethod]
        public async Task TestMessageReactionActivityRemoved()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.MessageReaction,
                ReactionsRemoved = new List<MessageReaction> { new MessageReaction { Type = "x" } },
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual(bot.Record[0], "OnMessageReactionActivityAsync");
            Assert.AreEqual(bot.Record[1], "OnMessageReactionsRemovedAsync");
        }

        [TestMethod]
        public async Task TestUnrecognizedActivityType()
        {
            // Arrange
            var activity = new Activity
            {
                Type = "shall.not.pass"
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(1, bot.Record.Count);
            Assert.AreEqual(bot.Record[0], "OnUnrecognizedActivityTypeAsync");
        }

        [TestMethod]
        public async Task TestTypingActivityType()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Typing
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(1, bot.Record.Count);
            Assert.AreEqual(bot.Record[0], "OnTypingActivityAsync");
        }

        [TestMethod]
        public async Task TestHandoffActivityType()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Handoff
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(1, bot.Record.Count);
            Assert.AreEqual(bot.Record[0], "OnHandoffActivityAsync");
        }

        [TestMethod]
        public async Task TestInstallationUpdateActivityType()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.InstallationUpdate
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(1, bot.Record.Count);
            Assert.AreEqual(bot.Record[0], "OnInstallationUpdateActivityAsync");
        }

        [TestMethod]
        public async Task TestDelegatingTurnContext()
        {
            // Arrange
            var turnContextMock = new Mock<ITurnContext>();
            turnContextMock.Setup(tc => tc.Activity).Returns(new Activity { Type = ActivityTypes.Message });
            turnContextMock.Setup(tc => tc.Adapter).Returns(new BotFrameworkAdapter(new SimpleCredentialProvider()));
            turnContextMock.Setup(tc => tc.TurnState).Returns(new TurnContextStateCollection());
            turnContextMock.Setup(tc => tc.Responded).Returns(false);
            turnContextMock.Setup(tc => tc.OnDeleteActivity(It.IsAny<DeleteActivityHandler>()));
            turnContextMock.Setup(tc => tc.OnSendActivities(It.IsAny<SendActivitiesHandler>()));
            turnContextMock.Setup(tc => tc.OnUpdateActivity(It.IsAny<UpdateActivityHandler>()));
            turnContextMock.Setup(tc => tc.SendActivityAsync(It.IsAny<IActivity>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new ResourceResponse()));
            turnContextMock.Setup(tc => tc.SendActivitiesAsync(It.IsAny<IActivity[]>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new[] { new ResourceResponse() }));
            turnContextMock.Setup(tc => tc.DeleteActivityAsync(It.IsAny<ConversationReference>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new ResourceResponse()));
            turnContextMock.Setup(tc => tc.UpdateActivityAsync(It.IsAny<IActivity>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new ResourceResponse()));

            // Act
            var bot = new TestDelegatingTurnContextActivityHandler();
            await bot.OnTurnAsync(turnContextMock.Object);

            // Assert
            turnContextMock.VerifyGet(tc => tc.Activity, Times.AtLeastOnce);
            turnContextMock.VerifyGet(tc => tc.Adapter, Times.Once);
            turnContextMock.VerifyGet(tc => tc.TurnState, Times.Once);
            turnContextMock.VerifyGet(tc => tc.Responded, Times.Once);
            turnContextMock.Verify(tc => tc.OnDeleteActivity(It.IsAny<DeleteActivityHandler>()), Times.Once);
            turnContextMock.Verify(tc => tc.OnSendActivities(It.IsAny<SendActivitiesHandler>()), Times.Once);
            turnContextMock.Verify(tc => tc.OnUpdateActivity(It.IsAny<UpdateActivityHandler>()), Times.Once);
            turnContextMock.Verify(tc => tc.SendActivityAsync(It.IsAny<IActivity>(), It.IsAny<CancellationToken>()), Times.Once);
            turnContextMock.Verify(tc => tc.SendActivitiesAsync(It.IsAny<IActivity[]>(), It.IsAny<CancellationToken>()), Times.Once);
            turnContextMock.Verify(tc => tc.DeleteActivityAsync(It.IsAny<ConversationReference>(), It.IsAny<CancellationToken>()), Times.Once);
            turnContextMock.Verify(tc => tc.UpdateActivityAsync(It.IsAny<IActivity>(), It.IsAny<CancellationToken>()), Times.Once);
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
        private class TestInvokeAdapter : NotImplementedAdapter
        {
            public IActivity Activity { get; private set; }

            public override Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
            {
                Activity = activities.FirstOrDefault(activity => activity.Type == ActivityTypesEx.InvokeResponse);
                return Task.FromResult(new ResourceResponse[0]);
            }
        }

        private class TestActivityHandler : ActivityHandler
        {
            public List<string> Record { get; } = new List<string>();

            protected override Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnMessageActivityAsync(turnContext, cancellationToken);
            }

            protected override Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
            }

            protected override Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnMembersAddedAsync(membersAdded, turnContext, cancellationToken);
            }

            protected override Task OnMembersRemovedAsync(IList<ChannelAccount> membersRemoved, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnMembersRemovedAsync(membersRemoved, turnContext, cancellationToken);
            }

            protected override Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnEventActivityAsync(turnContext, cancellationToken);
            }

            protected override Task OnTokenResponseEventAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnTokenResponseEventAsync(turnContext, cancellationToken);
            }

            protected override Task OnCreateConversationAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnCreateConversationAsync(turnContext, cancellationToken);
            }

            protected override Task OnContinueConversationAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnContinueConversationAsync(turnContext, cancellationToken);
            }

            protected override Task OnEventAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnEventAsync(turnContext, cancellationToken);
            }

            protected override Task OnContactRelationUpdateActivityAsync(ITurnContext<IContactRelationUpdateActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnContactRelationUpdateActivityAsync(turnContext, cancellationToken);
            }

            protected override Task OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnInvokeActivityAsync(turnContext, cancellationToken);
            }

            protected override Task<InvokeResponse> OnTeamsVerificationInvokeAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return Task.FromResult(new InvokeResponse { Status = (int)HttpStatusCode.OK });
            }

            protected override Task<InvokeResponse> OnInvokeAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return Task.FromResult(new InvokeResponse { Status = (int)HttpStatusCode.OK });
            }

            protected override Task OnEndOfConversationActivityAsync(ITurnContext<IEndOfConversationActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnEndOfConversationActivityAsync(turnContext, cancellationToken);
            }

            protected override Task OnDeleteUserDataActivityAsync(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnDeleteUserDataActivityAsync(turnContext, cancellationToken);
            }

            protected override Task OnMessageUpdateActivityAsync(ITurnContext<IMessageUpdateActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnMessageUpdateActivityAsync(turnContext, cancellationToken);
            }

            protected override Task OnMessageDeleteActivityAsync(ITurnContext<IMessageDeleteActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnMessageDeleteActivityAsync(turnContext, cancellationToken);
            }

            protected override Task OnMessageReactionActivityAsync(ITurnContext<IMessageReactionActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnMessageReactionActivityAsync(turnContext, cancellationToken);
            }

            protected override Task OnMessageReactionsAddedAsync(ITurnContext<IMessageReactionActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnMessageReactionsAddedAsync(turnContext, cancellationToken);
            }

            protected override Task OnMessageReactionsRemovedAsync(ITurnContext<IMessageReactionActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnMessageReactionsRemovedAsync(turnContext, cancellationToken);
            }

            protected override Task OnInstallationUpdateActivityAsync(ITurnContext<IInstallationUpdateActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnInstallationUpdateActivityAsync(turnContext, cancellationToken);
            }

            protected override Task OnHandoffActivityAsync(ITurnContext<IHandoffActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnHandoffActivityAsync(turnContext, cancellationToken);
            }

            protected override Task OnTypingActivityAsync(ITurnContext<ITypingActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnTypingActivityAsync(turnContext, cancellationToken);
            }

            protected override Task OnUnrecognizedActivityTypeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnUnrecognizedActivityTypeAsync(turnContext, cancellationToken);
            }
        }

        private class TestDelegatingTurnContextActivityHandler : ActivityHandler
        {
            protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
            {
                // touch every 
                var activity = turnContext.Activity;
                var adapter = turnContext.Adapter;
                var turnState = turnContext.TurnState;
                var responsed = turnContext.Responded;
                turnContext.OnDeleteActivity((t, a, n) => Task.CompletedTask);
                turnContext.OnSendActivities((t, a, n) => Task.FromResult(new ResourceResponse[] { new ResourceResponse() }));
                turnContext.OnUpdateActivity((t, a, n) => Task.FromResult(new ResourceResponse()));
                await turnContext.DeleteActivityAsync(activity.GetConversationReference());
                await turnContext.SendActivityAsync(new Activity());
                await turnContext.SendActivitiesAsync(new IActivity[] { new Activity() });
                await turnContext.UpdateActivityAsync(new Activity());
            }
        }
    }
}
