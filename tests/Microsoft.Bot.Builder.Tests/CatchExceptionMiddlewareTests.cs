// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Middleware;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public class CatchExceptionMiddlewareTests
    {
        [TestMethod]
        [TestCategory("Middleware")]
        public async Task CatchException_TestMiddleware()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new CatchExceptionMiddleware((context, phase, exception) =>
                    {
                        context.Reply("Sorry, something went wrong");
                        return Task.CompletedTask;
                    }));


            await new TestFlow(adapter, (context) =>
            {
                if (context.Request.AsMessageActivity().Text == "foo")
                {
                    context.Reply(context.Request.AsMessageActivity().Text);
                }
                if (context.Request.AsMessageActivity().Text == "error")
                {
                    throw new Exception();
                }
                return Task.CompletedTask;
            })
                .Send("foo")
                    .AssertReply("foo", "passthrough")
                .Send("error")
                    .AssertReply("Sorry, something went wrong")
                .StartTest();
        }

    }
}
