﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Tests
{
    public class ActivityHandlerTests
    {
        [Fact]
        public async Task TestMessageActivity()
        {
            // Arrange
            var activity = MessageFactory.Text("hello");
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.Single(bot.Record);
            Assert.Equal("OnMessageActivityAsync", bot.Record[0]);
        }

        [Fact]
        public async Task TestMessageUpdateActivity()
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.MessageUpdate };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.Single(bot.Record);
            Assert.Equal("OnMessageUpdateActivityAsync", bot.Record[0]);
        }

        [Fact]
        public async Task TestMessageDeleteActivity()
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.MessageDelete };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.Single(bot.Record);
            Assert.Equal("OnMessageDeleteActivityAsync", bot.Record[0]);
        }

        [Fact]
        public async Task TestEndOfConversationActivity()
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.EndOfConversation, Value = "some value" };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.Single(bot.Record);
            Assert.Equal("OnEndOfConversationActivityAsync", bot.Record[0]);
        }

        [Fact]
        public async Task TestTypingActivity()
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.Typing };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.Single(bot.Record);
            Assert.Equal("OnTypingActivityAsync", bot.Record[0]);
        }

        [Fact]
        public async Task TestInstallationUpdateActivity()
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.InstallationUpdate };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.Single(bot.Record);
            Assert.Equal("OnInstallationUpdateActivityAsync", bot.Record[0]);
        }

        [Fact]
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
            Assert.Single(bot.Record);
            Assert.Equal("OnConversationUpdateActivityAsync", bot.Record[0]);
        }

        [Fact]
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
            Assert.Equal(2, bot.Record.Count);
            Assert.Equal("OnConversationUpdateActivityAsync", bot.Record[0]);
            Assert.Equal("OnMembersAddedAsync", bot.Record[1]);
        }

        [Fact]
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
            Assert.Equal(2, bot.Record.Count);
            Assert.Equal("OnConversationUpdateActivityAsync", bot.Record[0]);
            Assert.Equal("OnMembersAddedAsync", bot.Record[1]);
        }

        [Fact]
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
            Assert.Single(bot.Record);
            Assert.Equal("OnConversationUpdateActivityAsync", bot.Record[0]);
        }

        [Fact]
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
            Assert.Equal(2, bot.Record.Count);
            Assert.Equal("OnConversationUpdateActivityAsync", bot.Record[0]);
            Assert.Equal("OnMembersRemovedAsync", bot.Record[1]);
        }

        [Fact]
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
            Assert.Equal(2, bot.Record.Count);
            Assert.Equal("OnConversationUpdateActivityAsync", bot.Record[0]);
            Assert.Equal("OnMembersRemovedAsync", bot.Record[1]);
        }

        [Fact]
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
            Assert.Single(bot.Record);
            Assert.Equal("OnConversationUpdateActivityAsync", bot.Record[0]);
        }

        [Fact]
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
            Assert.Single(bot.Record);
            Assert.Equal("OnConversationUpdateActivityAsync", bot.Record[0]);
        }

        [Fact]
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
            Assert.Equal(3, bot.Record.Count);
            Assert.Equal("OnMessageReactionActivityAsync", bot.Record[0]);
            Assert.Equal("OnReactionsAddedAsync", bot.Record[1]);
            Assert.Equal("OnReactionsRemovedAsync", bot.Record[2]);
        }

        [Fact]
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
            Assert.Equal(2, bot.Record.Count);
            Assert.Equal("OnEventActivityAsync", bot.Record[0]);
            Assert.Equal("OnTokenResponseEventAsync", bot.Record[1]);
        }

        [Fact]
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
            Assert.Equal(2, bot.Record.Count);
            Assert.Equal("OnEventActivityAsync", bot.Record[0]);
            Assert.Equal("OnEventAsync", bot.Record[1]);
        }

        [Fact]
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
            Assert.Single(bot.Record);
            Assert.Equal("OnInvokeActivityAsync", bot.Record[0]);
            Assert.Equal(200, ((InvokeResponse)((Activity)adapter.Activity).Value).Status);
        }

        [Fact]
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
            Assert.Equal(2, bot.Record.Count);
            Assert.Equal("OnInvokeActivityAsync", bot.Record[0]);
            Assert.Equal("OnSignInInvokeAsync", bot.Record[1]);
        }

        [Fact]
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
            Assert.Single(bot.Record);
            Assert.Equal("OnInvokeActivityAsync", bot.Record[0]);
            Assert.Equal(501, ((InvokeResponse)((Activity)adapter.Activity).Value).Status);
        }

        [Fact]
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
            Assert.Equal(2, bot.Record.Count);
            Assert.Equal("OnEventActivityAsync", bot.Record[0]);
            Assert.Equal("OnEventAsync", bot.Record[1]);
        }

        [Fact]
        public async Task TestInstallationUpdateAddAsync()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.InstallationUpdate,
                Action = "add"
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.Equal(2, bot.Record.Count);
            Assert.Equal("OnInstallationUpdateActivityAsync", bot.Record[0]);
            Assert.Equal("OnInstallationUpdateAddAsync", bot.Record[1]);
        }

        [Fact]
        public async Task TestInstallationUpdateAddUpgradeAsync()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.InstallationUpdate,
                Action = "add-upgrade"
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.Equal(2, bot.Record.Count);
            Assert.Equal("OnInstallationUpdateActivityAsync", bot.Record[0]);
            Assert.Equal("OnInstallationUpdateAddAsync", bot.Record[1]);
        }

        [Fact]
        public async Task TestInstallationUpdateRemoveAsync()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.InstallationUpdate,
                Action = "remove"
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.Equal(2, bot.Record.Count);
            Assert.Equal("OnInstallationUpdateActivityAsync", bot.Record[0]);
            Assert.Equal("OnInstallationUpdateRemoveAsync", bot.Record[1]);
        }

        [Fact]
        public async Task TestInstallationUpdateRemoveUpgradeAsync()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.InstallationUpdate,
                Action = "remove-upgrade"
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.Equal(2, bot.Record.Count);
            Assert.Equal("OnInstallationUpdateActivityAsync", bot.Record[0]);
            Assert.Equal("OnInstallationUpdateRemoveAsync", bot.Record[1]);
        }

        [Fact]
        public async Task TestOnAdaptiveCardInvokeAsync()
        {
            var value = JObject.FromObject(new AdaptiveCardInvokeValue { Action = new AdaptiveCardInvokeAction { Type = "Action.Execute" } });

            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "adaptiveCard/action",
                Value = value
            };

            var turnContext = new TurnContext(new TestInvokeAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.Equal(2, bot.Record.Count);
            Assert.Equal("OnInvokeActivityAsync", bot.Record[0]);
            Assert.Equal("OnAdaptiveCardInvokeAsync", bot.Record[1]);
        }

        [Fact]
        public async Task TestCommandActivityType()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Command,
                Name = "application/test",
                Value = new CommandValue<object> { CommandId = "Test", Data = new { test = true } }
            };
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.Single(bot.Record);
            Assert.Equal("OnCommandActivityAsync", bot.Record[0]);
        }

        [Fact]
        public async Task TestCommandResultActivityType()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.CommandResult,
                Name = "application/test",
                Value = new CommandResultValue<object> { CommandId = "Test", Data = new { test = true } }
            };

            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.Single(bot.Record);
            Assert.Equal("OnCommandResultActivityAsync", bot.Record[0]);
        }

        [Fact]
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
            Assert.Single(bot.Record);
            Assert.Equal("OnUnrecognizedActivityTypeAsync", bot.Record[0]);
        }

        [Fact]
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

        [Fact]
        public async Task TestOnSearchInvokeAsync()
        {
            // Arrange
            var value = JObject.FromObject(new SearchInvokeValue { Kind = SearchInvokeTypes.Search, QueryText = "bot" });
            var activity = GetSearchActivity(value);
            var turnContext = new TurnContext(new TestInvokeAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.Equal(2, bot.Record.Count);
            Assert.Equal("OnInvokeActivityAsync", bot.Record[0]);
            Assert.Equal("OnSearchInvokeAsync", bot.Record[1]);
        }

        [Fact]
        public async Task TestOnSearchInvokeAsync_NoKindOnTeamsDefaults()
        {
            // Arrange
            var value = JObject.FromObject(new SearchInvokeValue { Kind = null, QueryText = "bot" });
            var activity = GetSearchActivity(value);
            activity.ChannelId = Channels.Msteams;
            var turnContext = new TurnContext(new TestInvokeAdapter(), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.Equal(2, bot.Record.Count);
            Assert.Equal("OnInvokeActivityAsync", bot.Record[0]);
            Assert.Equal("OnSearchInvokeAsync", bot.Record[1]);
        }

        [Fact]
        public async Task TestGetSearchInvokeValue_NullValueThrows()
        {
            var activity = GetSearchActivity(null);
            await AssertErrorThroughInvokeAdapter(activity, "Missing value property for search");
        }

        [Fact]
        public async Task TestGetSearchInvokeValue_InvalidValueThrows()
        {
            var activity = GetSearchActivity(new object());
            await AssertErrorThroughInvokeAdapter(activity, "Value property is not properly formed for search");
        }

        [Fact]
        public async Task TestGetSearchInvokeValue_MissingKindThrows()
        {
            var activity = GetSearchActivity(JObject.FromObject(new SearchInvokeValue { Kind = null, QueryText = "test" }));
            await AssertErrorThroughInvokeAdapter(activity, "Missing kind property for search");
        }

        [Fact]
        public async Task TestGetSearchInvokeValue_MissingQueryTextThrows()
        {
            var activity = GetSearchActivity(JObject.FromObject(new SearchInvokeValue { Kind = SearchInvokeTypes.Typeahead }));
            await AssertErrorThroughInvokeAdapter(activity, "Missing queryText property for search");
        }

        private Activity GetSearchActivity(object value)
        {
            return new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "application/search",
                Value = value
            };
        }

        private async Task AssertErrorThroughInvokeAdapter(Activity activity, string errorMessage)
        {
            // Arrange
            var adapter = new TestInvokeAdapter();
            var turnContext = new TurnContext(adapter, activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            var sent = adapter.Activity as Activity;
            Assert.Equal(ActivityTypesEx.InvokeResponse, sent.Type);

            Assert.IsType<InvokeResponse>(sent.Value);
            var value = sent.Value as InvokeResponse;
            Assert.Equal(400, value.Status);

            Assert.IsType<AdaptiveCardInvokeResponse>(value.Body);
            var body = value.Body as AdaptiveCardInvokeResponse;
            Assert.Equal("application/vnd.microsoft.error", body.Type);
            Assert.Equal(400, body.StatusCode);

            Assert.IsType<Error>(body.Value);
            var error = body.Value as Error;
            Assert.Equal("BadRequest", error.Code);
            Assert.Equal(errorMessage, error.Message);
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

            protected override Task OnInstallationUpdateActivityAsync(ITurnContext<IInstallationUpdateActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnInstallationUpdateActivityAsync(turnContext, cancellationToken);
            }

            protected override Task OnCommandActivityAsync(ITurnContext<ICommandActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnCommandActivityAsync(turnContext, cancellationToken);
            }

            protected override Task OnCommandResultActivityAsync(ITurnContext<ICommandResultActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnCommandResultActivityAsync(turnContext, cancellationToken);
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

            protected override Task OnInstallationUpdateAddAsync(ITurnContext<IInstallationUpdateActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnInstallationUpdateAddAsync(turnContext, cancellationToken);
            }

            protected override Task OnInstallationUpdateRemoveAsync(ITurnContext<IInstallationUpdateActivity> turnContext, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return base.OnInstallationUpdateRemoveAsync(turnContext, cancellationToken);
            }

            protected override Task<AdaptiveCardInvokeResponse> OnAdaptiveCardInvokeAsync(ITurnContext<IInvokeActivity> turnContext, AdaptiveCardInvokeValue invokeValue, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return Task.FromResult(new AdaptiveCardInvokeResponse());
            }

            protected override Task<SearchInvokeResponse> OnSearchInvokeAsync(ITurnContext<IInvokeActivity> turnContext, SearchInvokeValue invokeValue, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return Task.FromResult(new SearchInvokeResponse());
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
