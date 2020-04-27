// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            Assert.AreEqual("OnMessageActivityAsync", bot.Record[0]);
        }

        [TestMethod]
        public async Task TestEndOfConversationActivity()
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.EndOfConversation, Value = "some value" };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(1, bot.Record.Count);
            Assert.AreEqual("OnEndOfConversationActivityAsync", bot.Record[0]);
        }

        [TestMethod]
        public async Task TestTypingActivity()
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.Typing };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(1, bot.Record.Count);
            Assert.AreEqual("OnTypingActivityAsync", bot.Record[0]);
        }

        [TestMethod]
        public async Task TestMemberAdded1()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.ConversationUpdate,
                MembersAdded = new List<ChannelAccount>
                {
                    new ChannelAccount { Id = "b" },
                },
                Recipient = new ChannelAccount { Id = "b" },
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(1, bot.Record.Count);
            Assert.AreEqual("OnConversationUpdateActivityAsync", bot.Record[0]);
        }

        [TestMethod]
        public async Task TestMemberAdded2()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.ConversationUpdate,
                MembersAdded = new List<ChannelAccount>
                {
                    new ChannelAccount { Id = "a" },
                    new ChannelAccount { Id = "b" },
                },
                Recipient = new ChannelAccount { Id = "b" },
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual("OnConversationUpdateActivityAsync", bot.Record[0]);
            Assert.AreEqual("OnMembersAddedAsync", bot.Record[1]);
        }

        [TestMethod]
        public async Task TestMemberAdded3()
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
                Recipient = new ChannelAccount { Id = "b" },
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual("OnConversationUpdateActivityAsync", bot.Record[0]);
            Assert.AreEqual("OnMembersAddedAsync", bot.Record[1]);
        }

        [TestMethod]
        public async Task TestMemberRemoved1()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.ConversationUpdate,
                MembersRemoved = new List<ChannelAccount>
                {
                    new ChannelAccount { Id = "c" },
                },
                Recipient = new ChannelAccount { Id = "c" },
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(1, bot.Record.Count);
            Assert.AreEqual("OnConversationUpdateActivityAsync", bot.Record[0]);
        }

        [TestMethod]
        public async Task TestMemberRemoved2()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.ConversationUpdate,
                MembersRemoved = new List<ChannelAccount>
                {
                    new ChannelAccount { Id = "a" },
                    new ChannelAccount { Id = "c" },
                },
                Recipient = new ChannelAccount { Id = "c" },
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual("OnConversationUpdateActivityAsync", bot.Record[0]);
            Assert.AreEqual("OnMembersRemovedAsync", bot.Record[1]);
        }

        [TestMethod]
        public async Task TestMemberRemoved3()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.ConversationUpdate,
                MembersRemoved = new List<ChannelAccount>
                {
                    new ChannelAccount { Id = "a" },
                    new ChannelAccount { Id = "b" },
                    new ChannelAccount { Id = "c" },
                },
                Recipient = new ChannelAccount { Id = "c" },
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual("OnConversationUpdateActivityAsync", bot.Record[0]);
            Assert.AreEqual("OnMembersRemovedAsync", bot.Record[1]);
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
                Recipient = new ChannelAccount { Id = "b" },
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(1, bot.Record.Count);
            Assert.AreEqual("OnConversationUpdateActivityAsync", bot.Record[0]);
        }

        [TestMethod]
        public async Task TestMemberRemovedJustTheBot()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.ConversationUpdate,
                MembersRemoved = new List<ChannelAccount>
                {
                    new ChannelAccount { Id = "c" },
                },
                Recipient = new ChannelAccount { Id = "c" },
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(1, bot.Record.Count);
            Assert.AreEqual("OnConversationUpdateActivityAsync", bot.Record[0]);
        }

        [TestMethod]
        public async Task TestMessageReaction()
        {
            // Note the code supports multiple adds and removes in the same activity though
            // a channel may decide to send separate activities for each. For example, Teams
            // sends separate activities each with a single add and a single remove.

            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.MessageReaction,
                ReactionsAdded = new List<MessageReaction>
                {
                    new MessageReaction("sad"),
                },
                ReactionsRemoved = new List<MessageReaction>
                {
                    new MessageReaction("angry"),
                },
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(3, bot.Record.Count);
            Assert.AreEqual("OnMessageReactionActivityAsync", bot.Record[0]);
            Assert.AreEqual("OnReactionsAddedAsync", bot.Record[1]);
            Assert.AreEqual("OnReactionsRemovedAsync", bot.Record[2]);
        }

        [TestMethod]
        public async Task TestTokenResponseEventAsync()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Event,
                Name = SignInConstants.TokenResponseEventName,
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual("OnEventActivityAsync", bot.Record[0]);
            Assert.AreEqual("OnTokenResponseEventAsync", bot.Record[1]);
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
            Assert.AreEqual("OnEventActivityAsync", bot.Record[0]);
            Assert.AreEqual("OnEventAsync", bot.Record[1]);
        }

        [TestMethod]
        public async Task TestInvokeAsync()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "some.random.invoke",
            };

            var adapter = new TestInvokeAdapter();
            var turnContext = new TurnContext(adapter, activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(1, bot.Record.Count);
            Assert.AreEqual("OnInvokeActivityAsync", bot.Record[0]);
            Assert.AreEqual(200, ((InvokeResponse)((Activity)adapter.Activity).Value).Status);
        }

        [TestMethod]
        public async Task TestSignInInvokeAsync()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = SignInConstants.VerifyStateOperationName,
            };
            var turnContext = new TurnContext(new TestInvokeAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual("OnInvokeActivityAsync", bot.Record[0]);
            Assert.AreEqual("OnSignInInvokeAsync", bot.Record[1]);
        }

        [TestMethod]
        public async Task TestHealthCheckAsyncOverride()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "healthCheck",
            };
            var turnContext = new TurnContext(new TestInvokeAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual("OnInvokeActivityAsync", bot.Record[0]);
            Assert.AreEqual("OnHealthCheckAsync", bot.Record[1]);
        }

        [TestMethod]
        public async Task TestHealthCheckAsync()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "healthCheck"
            };

            Activity[] activitiesToSend = null;
            void CaptureSend(Activity[] arg)
            {
                activitiesToSend = arg;
            }

            var turnContext = new TurnContext(new SimpleAdapter(CaptureSend), activity);

            // Act
            var bot = new ActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.IsNotNull(activitiesToSend);
            Assert.AreEqual(1, activitiesToSend.Length);
            Assert.IsInstanceOfType(activitiesToSend[0].Value, typeof(InvokeResponse));
            Assert.AreEqual(200, ((InvokeResponse)activitiesToSend[0].Value).Status);

            using (var writer = new StringWriter())
            {
                using (var jsonWriter = new JsonTextWriter(writer))
                {
                    var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                    var serializer = JsonSerializer.Create(settings);

                    serializer.Serialize(jsonWriter, ((InvokeResponse)activitiesToSend[0].Value).Body);
                }

                await writer.FlushAsync();

                var obj = JObject.Parse(writer.ToString());

                Assert.IsTrue(obj["healthResults"]["success"].Value<bool>());
                Assert.AreEqual("Health check succeeded.", obj["healthResults"]["messages"][0].ToString());
            }
        }

        [TestMethod]
        public async Task TestHealthCheckWithConnectorAsync()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "healthCheck"
            };

            Activity[] activitiesToSend = null;
            void CaptureSend(Activity[] arg)
            {
                activitiesToSend = arg;
            }

            var turnContext = new TurnContext(new SimpleAdapter(CaptureSend), activity);

            IConnectorClient connector = new MockConnectorClient();

            turnContext.TurnState.Add(connector);

            // Act
            var bot = new ActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.IsNotNull(activitiesToSend);
            Assert.AreEqual(1, activitiesToSend.Length);
            Assert.IsInstanceOfType(activitiesToSend[0].Value, typeof(InvokeResponse));
            Assert.AreEqual(200, ((InvokeResponse)activitiesToSend[0].Value).Status);

            using (var writer = new StringWriter())
            {
                using (var jsonWriter = new JsonTextWriter(writer))
                {
                    var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                    var serializer = JsonSerializer.Create(settings);

                    serializer.Serialize(jsonWriter, ((InvokeResponse)activitiesToSend[0].Value).Body);
                }

                await writer.FlushAsync();

                var obj = JObject.Parse(writer.ToString());

                Assert.IsTrue(obj["healthResults"]["success"].Value<bool>());
                Assert.AreEqual("awesome", obj["healthResults"]["authorization"].ToString());
                Assert.AreEqual("Windows/3.1", obj["healthResults"]["user-agent"].ToString());
                Assert.AreEqual("Health check succeeded.", obj["healthResults"]["messages"][0].ToString());
            }
        }

        [TestMethod]
        public async Task TestInvokeShouldNotMatchAsync()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "should.not.match",
            };
            var adapter = new TestInvokeAdapter();
            var turnContext = new TurnContext(adapter, activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(1, bot.Record.Count);
            Assert.AreEqual("OnInvokeActivityAsync", bot.Record[0]);
            Assert.AreEqual(501, ((InvokeResponse)((Activity)adapter.Activity).Value).Status);
        }

        [TestMethod]
        public async Task TestEventNullNameAsync()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Event,
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(2, bot.Record.Count);
            Assert.AreEqual("OnEventActivityAsync", bot.Record[0]);
            Assert.AreEqual("OnEventAsync", bot.Record[1]);
        }

        [TestMethod]
        public async Task TestUnrecognizedActivityType()
        {
            // Arrange
            var activity = new Activity
            {
                Type = "shall.not.pass",
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.AreEqual(1, bot.Record.Count);
            Assert.AreEqual("OnUnrecognizedActivityTypeAsync", bot.Record[0]);
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

            protected override Task OnMessageReactionActivityAsync(ITurnContext<IMessageReactionActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnMessageReactionActivityAsync(turnContext, cancellationToken);
            }

            protected override Task OnReactionsAddedAsync(IList<MessageReaction> messageReactions, ITurnContext<IMessageReactionActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnReactionsAddedAsync(messageReactions, turnContext, cancellationToken);
            }

            protected override Task OnReactionsRemovedAsync(IList<MessageReaction> messageReactions, ITurnContext<IMessageReactionActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnReactionsRemovedAsync(messageReactions, turnContext, cancellationToken);
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

            protected override Task OnEventAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnEventAsync(turnContext, cancellationToken);
            }

            protected override Task OnEndOfConversationActivityAsync(ITurnContext<IEndOfConversationActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnEndOfConversationActivityAsync(turnContext, cancellationToken);
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

            protected override Task<InvokeResponse> OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                if (turnContext.Activity.Name == "some.random.invoke")
                {
                    return Task.FromResult(CreateInvokeResponse());
                }

                return base.OnInvokeActivityAsync(turnContext, cancellationToken);
            }

            protected override Task OnSignInInvokeAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return Task.CompletedTask;
            }

            protected override Task<HealthCheckResponse> OnHealthCheckAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return Task.FromResult(new HealthCheckResponse());
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

        private class MockConnectorClient : IConnectorClient
        {
            private Uri _baseUri = new Uri("http://tempuri.org/whatever");

            public Uri BaseUri
            {
                get { return _baseUri; }
                set { _baseUri = value; }
            }

            public JsonSerializerSettings SerializationSettings => throw new NotImplementedException();

            public JsonSerializerSettings DeserializationSettings => throw new NotImplementedException();

            public ServiceClientCredentials Credentials { get => new MockCredentials(); }

            public IAttachments Attachments => throw new NotImplementedException();

            public IConversations Conversations => throw new NotImplementedException();

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            private class MockCredentials : ServiceClientCredentials
            {
                public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("awesome");
                    request.Headers.UserAgent.Add(new ProductInfoHeaderValue("Windows", "3.1"));
                    return Task.CompletedTask;
                }
            }
        }
    }
}
