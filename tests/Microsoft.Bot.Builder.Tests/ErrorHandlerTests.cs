// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Core.Extensions.Tests
{
    [TestClass]
    public class ErrorHandlerTests
    {
        [TestMethod]
        [TestCategory("Middleware")]
        public async Task ErrorHandler_Test()
        {
            TestAdapter adapter = new TestAdapter();
            adapter.ErrorHandler = async (context, exception) =>
            {
                if (exception is NotImplementedException)
                {
                    await context.SendActivity(context.Activity.CreateReply(exception.Message));
                }
                else
                {
                    await context.SendActivity("Unexpected exception");
                }
            };

            await new TestFlow(adapter, (context) =>
                {
                    if (context.Activity.AsMessageActivity().Text == "foo")
                    {
                        context.SendActivity(context.Activity.AsMessageActivity().Text);
                    }

                    if (context.Activity.AsMessageActivity().Text == "NotImplementedException")
                    {
                        throw new NotImplementedException("Test");
                    }

                    return Task.CompletedTask;
                })
                .Send("foo")
                .AssertReply("foo", "passthrough")
                .Send("NotImplementedException")
                .AssertReply("Test")
                .StartTest();
        }
    }
}
