// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Xunit;

namespace Microsoft.Bot.Builder.Tests
{
    public class ShowTyping_MiddlewareTests
    {
        /// <summary>
        /// Enum to handle different test cases.
        /// </summary>
        public enum FlowTestCase
        {
            /// <summary>
            /// RunAsync is executing on a root bot with no skills (typical standalone bot).
            /// </summary>
            RootBot,

            /// <summary>
            /// RunAsync is executing in a skill.
            /// </summary>
            Skill
        }

        [Fact]
        public async Task ShowTyping_TestMiddleware_1_Second_Interval()
        {
            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation("ShowTyping_TestMiddleware_1_Second_Interval"))
                .Use(new MockBotIdentityMiddleware(FlowTestCase.RootBot))
                .Use(new ShowTypingMiddleware(100, 1000));

            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(2800));

                // note the ShowTypingMiddleware should not cause the Responded flag to be set
                Assert.False(context.Responded);

                await context.SendActivityAsync("Message sent after delay");
                await Task.CompletedTask;
            })
                .Send("foo")
                .AssertReply(ValidateTypingActivity, "check typing activity")
                .AssertReply(ValidateTypingActivity, "check typing activity")
                .AssertReply(ValidateTypingActivity, "check typing activity")
                .AssertReply("Message sent after delay")
                .StartTestAsync();
        }

        [Fact]
        public async Task ShowTyping_TestMiddleware_Context_Completes_Before_Typing_Interval()
        {
            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation("ShowTyping_TestMiddleware_Context_Completes_Before_Typing_Interval"))
                .Use(new MockBotIdentityMiddleware(FlowTestCase.RootBot))
                .Use(new ShowTypingMiddleware(100, 5000));

            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(2000));
                await context.SendActivityAsync("Message sent after delay");
                await Task.CompletedTask;
            })
                .Send("foo")
                .AssertReply(ValidateTypingActivity, "check typing activity")
                .AssertReply("Message sent after delay")
                .StartTestAsync();
        }

        [Fact]
        public async Task ShowTyping_TestMiddleware_ImmediateResponse_5SecondInterval()
        {
            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation("ShowTyping_TestMiddleware_ImmediateResponse_5SecondInterval"))
                .Use(new MockBotIdentityMiddleware(FlowTestCase.RootBot))
                .Use(new ShowTypingMiddleware(2000, 5000));

            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                await context.SendActivityAsync("Message sent after delay");
                await Task.CompletedTask;
            })
                .Send("foo")
                .AssertReply("Message sent after delay")
                .StartTestAsync();
        }

        [Fact]
        public async Task ShowTyping_TestMiddleware_ImmediateResponse_When_Running_As_Skill()
        {
            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation("ShowTyping_TestMiddleware_1_Second_Interval"))
                .Use(new MockBotIdentityMiddleware(FlowTestCase.Skill))
                .Use(new ShowTypingMiddleware(100, 1000));

            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(2800));

                // note the ShowTypingMiddleware should not cause the Responded flag to be set
                Assert.False(context.Responded);

                await context.SendActivityAsync("Message sent after delay");
                await Task.CompletedTask;
            })
                .Send("foo")
                .AssertReply("Message sent after delay")
                .StartTestAsync();
        }

        [Fact]
        public void ShowTyping_TestMiddleware_NegativeDelay()
        {
            try
            {
                TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation("ShowTyping_TestMiddleware_NegativeDelay"))
                    .Use(new MockBotIdentityMiddleware(FlowTestCase.RootBot))
                    .Use(new ShowTypingMiddleware(-100, 1000));
            }
            catch (Exception ex)
            {
                Assert.IsType<ArgumentOutOfRangeException>(ex);
            }
        }

        [Fact]
        public void ShowTyping_TestMiddleware_ZeroFrequency()
        {
            try
            {
                TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation("ShowTyping_TestMiddleware_ZeroFrequency"))
                    .Use(new ShowTypingMiddleware(-100, 0));
            }
            catch (Exception ex)
            {
                Assert.IsType<ArgumentOutOfRangeException>(ex);
            }
        }

        private void ValidateTypingActivity(IActivity obj)
        {
            var activity = obj.AsTypingActivity();
            if (activity != null)
            {
                return;
            }
            else
            {
                throw new Exception("Activity was not of type TypingActivity");
            }
        }

        private class MockBotIdentityMiddleware : IMiddleware
        {
            private readonly FlowTestCase _flowTestCase;

            // An App ID for a parent bot.
            private readonly string _parentBotId = Guid.NewGuid().ToString();

            // An App ID for a skill bot.
            private readonly string _skillBotId = Guid.NewGuid().ToString();

            public MockBotIdentityMiddleware(FlowTestCase flowTestCase)
            {
                _flowTestCase = flowTestCase;
            }

            public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
            {
                // Create a skill ClaimsIdentity and put it in TurnState so SkillValidation.IsSkillClaim() returns true.
                var claimsIdentity = new ClaimsIdentity();
                claimsIdentity.AddClaim(new Claim(AuthenticationConstants.VersionClaim, "2.0"));
                claimsIdentity.AddClaim(new Claim(AuthenticationConstants.AudienceClaim, _parentBotId));

                if (_flowTestCase == FlowTestCase.Skill)
                {
                    claimsIdentity.AddClaim(new Claim(AuthenticationConstants.AuthorizedParty, _skillBotId));
                }

                turnContext.TurnState.Add(BotAdapter.BotIdentityKey, claimsIdentity);

                await next(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
