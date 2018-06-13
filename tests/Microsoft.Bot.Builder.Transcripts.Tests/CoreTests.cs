// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions.Tests;
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

            TestAdapter adapter = new TestAdapter()
                .Use(new BeforeAfterMiddleware())
                .Use(new CatchExceptionMiddleware());

            var flow = new TestFlow(adapter, async (context) => {
                var userMessage = context.Activity.AsMessageActivity()?.Text;
                switch (userMessage)
                {
                    case "use middleware":
                        await context.SendActivity("using middleware");
                        break;
                    case "catch exception":
                        await context.SendActivity("generating exception");
                        throw new Exception("exception to catch");
                }
            });

            await flow.Test(activities).StartTest();
        }

        public class BeforeAfterMiddleware : IMiddleware
        {
            public async Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
            {
                await context.SendActivity("before message");
                await next();
                await context.SendActivity("after message");
            }
        }

        public class CatchExceptionMiddleware : IMiddleware
        {
            public async Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
            {
                try
                {
                    await next();
                }
                catch (Exception ex)
                {
                    await context.SendActivity($"Caught: {ex.Message}");
                }
            }
        }
    }
}
