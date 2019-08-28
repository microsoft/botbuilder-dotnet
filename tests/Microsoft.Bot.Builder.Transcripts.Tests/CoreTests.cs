// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Tests;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Transcripts.Tests
{
    [TestClass]
    public class CoreTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task BotAdapted_Bracketing()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new BeforeAfterMiddleware());
            adapter.OnTurnError = async (context, exception) =>
            {
                await context.SendActivityAsync($"Caught: {exception.Message}");
                return;
            };

            var flow = new TestFlow(adapter, async (context, cancellationToken) =>
            {
                switch (context.Activity.Type)
                {
                    case ActivityTypes.Message:
                        {
                            var userMessage = context.Activity.AsMessageActivity()?.Text;
                            switch (userMessage)
                            {
                                case "use middleware":
                                    await context.SendActivityAsync("using middleware");
                                    break;
                                case "catch exception":
                                    await context.SendActivityAsync("generating exception");
                                    throw new Exception("exception to catch");
                            }
                        }

                        break;
                    default:
                        await context.SendActivityAsync(context.Activity.Type);
                        break;
                }
            });

            await flow.Test(activities).StartTestAsync();
        }

        public class BeforeAfterMiddleware : IMiddleware
        {
            public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
            {
                await turnContext.SendActivityAsync("before message");
                await next(cancellationToken);
                await turnContext.SendActivityAsync("after message");
            }
        }
    }
}
