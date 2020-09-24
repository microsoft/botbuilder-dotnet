// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Xunit;

namespace Microsoft.Bot.Builder.Tests
{
    public class BotAdapterBracketingTest
    {
        /// <summary>
        /// Developer authored Middleware that looks like this:
        /// public async Task ReceiveActivityAsync(ITurnContext turnContext,
        ///    MiddlewareSet.NextDelegate next)
        /// {
        ///    context.Reply("BEFORE");
        ///    await next();   // User Says Hello
        ///    context.Reply("AFTER");
        ///  }
        ///  Should result in an output that looks like:
        ///  BEFORE
        ///  ECHO:Hello
        ///  AFTER.
        ///  </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Middlware_BracketingValidation()
        {
            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation("Middlware_BracketingValidation"))
                .Use(new BeforeAFterMiddlware());

            async Task Echo(ITurnContext ctx, CancellationToken cancellationToken)
            {
                string toEcho = "ECHO:" + ctx.Activity.AsMessageActivity().Text;
                await ctx.SendActivityAsync(ctx.Activity.CreateReply(toEcho), cancellationToken);
            }

            await new TestFlow(adapter, Echo)
                .Send("test")
                .AssertReply("BEFORE")
                .AssertReply("ECHO:test")
                .AssertReply("AFTER")
                .StartTestAsync();
        }

        /// <summary>
        /// Exceptions thrown during the processing of an Activity should
        /// be catchable by Middleware that has wrapped the next() method.
        /// This tests verifies that, and makes sure the order of messages
        /// coming back is correct.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Middlware_ThrowException()
        {
            string uniqueId = Guid.NewGuid().ToString();

            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation("Middlware_ThrowException"))
                .Use(new CatchExceptionMiddleware());

            async Task EchoWithException(ITurnContext ctx, CancellationToken cancellationToken)
            {
                string toEcho = "ECHO:" + ctx.Activity.AsMessageActivity().Text;
                await ctx.SendActivityAsync(ctx.Activity.CreateReply(toEcho));
                throw new Exception(uniqueId);
            }

            await new TestFlow(adapter, EchoWithException)
                .Send("test")
                .AssertReply("BEFORE")
                .AssertReply("ECHO:test")
                .AssertReply("CAUGHT:" + uniqueId)
                .AssertReply("AFTER")
                .StartTestAsync();
        }

        public class CatchExceptionMiddleware : IMiddleware
        {
            public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken)
            {
                await turnContext.SendActivityAsync(turnContext.Activity.CreateReply("BEFORE"));
                try
                {
                    await next(cancellationToken);
                }
                catch (Exception ex)
                {
                    await turnContext.SendActivityAsync(turnContext.Activity.CreateReply("CAUGHT:" + ex.Message));
                }

                await turnContext.SendActivityAsync(turnContext.Activity.CreateReply("AFTER"));
            }
        }

        public class BeforeAFterMiddlware : IMiddleware
        {
            public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken)
            {
                await turnContext.SendActivityAsync(turnContext.Activity.CreateReply("BEFORE"));
                await next(cancellationToken);
                await turnContext.SendActivityAsync(turnContext.Activity.CreateReply("AFTER"));
            }
        }
    }
}
