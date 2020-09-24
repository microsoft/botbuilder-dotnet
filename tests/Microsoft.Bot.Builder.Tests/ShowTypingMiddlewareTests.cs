// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Xunit;

namespace Microsoft.Bot.Builder.Tests
{
    public class ShowTypingMiddlewareTests
    {
        [Fact]
        public void ConstructorValidation()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ShowTypingMiddleware(-100, 1000));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ShowTypingMiddleware(100, -1000));
        }

        [Fact]
        public async Task OneSecondInterval()
        {
            var adapter = new TestAdapter(TestAdapter.CreateConversation("One_Second_Interval"))
                .Use(new ShowTypingMiddleware(100, 1000));

            await new TestFlow(adapter, async (context, cancellationToken) =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(2800), cancellationToken);

                    // note the ShowTypingMiddleware should not cause the Responded flag to be set
                    Assert.False(context.Responded);

                    await context.SendActivityAsync("Message sent after delay", cancellationToken: cancellationToken);
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
        public async Task ContextCompletesBeforeTypingInterval()
        {
            var adapter = new TestAdapter(TestAdapter.CreateConversation("Context_Completes_Before_Typing_Interval"))
                .Use(new ShowTypingMiddleware(100, 5000));

            await new TestFlow(adapter, async (context, cancellationToken) =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(2000), cancellationToken);
                    await context.SendActivityAsync("Message sent after delay", cancellationToken: cancellationToken);
                    await Task.CompletedTask;
                })
                .Send("foo")
                .AssertReply(ValidateTypingActivity, "check typing activity")
                .AssertReply("Message sent after delay")
                .StartTestAsync();
        }

        [Fact]
        public async Task ImmediateResponseFiveSecondInterval()
        {
            var adapter = new TestAdapter(TestAdapter.CreateConversation("ImmediateResponse_5SecondInterval"))
                .Use(new ShowTypingMiddleware(2000, 5000));

            await new TestFlow(adapter, async (context, cancellationToken) =>
                {
                    await context.SendActivityAsync("Message sent after delay", cancellationToken: cancellationToken);
                    await Task.CompletedTask;
                })
                .Send("foo")
                .AssertReply("Message sent after delay")
                .StartTestAsync();
        }

        [Fact]
        public async Task ImmediateResponseWhenRunningAsSkill()
        {
            var adapter = new SkillTestAdapter(TestAdapter.CreateConversation("1_Second_Interval"))
                .Use(new ShowTypingMiddleware(100, 1000));

            await new TestFlow(adapter, async (context, cancellationToken) =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(2800), cancellationToken);

                    // note the ShowTypingMiddleware should not cause the Responded flag to be set
                    Assert.False(context.Responded);

                    await context.SendActivityAsync("Message sent after delay", cancellationToken: cancellationToken);
                    await Task.CompletedTask;
                })
                .Send("foo")
                .AssertReply("Message sent after delay")
                .StartTestAsync();
        }

        [Fact]
        public void ZeroFrequency()
        {
            try
            {
                _ = new TestAdapter(TestAdapter.CreateConversation("ZeroFrequency"))
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

            throw new Exception("Activity was not of type TypingActivity");
        }

        /// <summary>
        /// A helper TestAdapter that injects skill claims in the turn so we can test skill use cases.
        /// </summary>
        private class SkillTestAdapter : TestAdapter
        {
            // An App ID for a parent bot.
            private static readonly string _parentBotId = Guid.NewGuid().ToString();

            // An App ID for a skill bot.
            private static readonly string _skillBotId = Guid.NewGuid().ToString();

            public SkillTestAdapter(ConversationReference conversation = null)
                : base(conversation)
            {
            }

            protected override TurnContext CreateTurnContext(Activity activity)
            {
                // Get the default turnContext from the base.
                var turnContext = base.CreateTurnContext(activity);

                // Create a skill ClaimsIdentity and put it in TurnState so SkillValidation.IsSkillClaim() returns true.
                var claimsIdentity = new ClaimsIdentity();
                claimsIdentity.AddClaim(new Claim(AuthenticationConstants.VersionClaim, "2.0"));
                claimsIdentity.AddClaim(new Claim(AuthenticationConstants.AudienceClaim, _parentBotId));
                claimsIdentity.AddClaim(new Claim(AuthenticationConstants.AuthorizedParty, _skillBotId));
                turnContext.TurnState.Add(BotIdentityKey, claimsIdentity);

                return turnContext;
            }
        }
    }
}
