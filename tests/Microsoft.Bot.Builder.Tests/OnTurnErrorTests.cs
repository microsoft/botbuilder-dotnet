// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Core.Extensions.Tests
{
    [TestClass]
    public class OnTurnErrorTests
    {
        [TestMethod]
        [TestCategory("Middleware")]
        public async Task OnTurnError_Test()
        {
            TestAdapter adapter = new TestAdapter();
            adapter.OnTurnError = async (context, exception) =>
            {
                if (exception is NotImplementedException)
                {
                    await context.SendActivityAsync(context.Activity.CreateReply(exception.Message));
                }
                else
                {
                    await context.SendActivityAsync("Unexpected exception");
                }
            };

            await new TestFlow(adapter, (context) =>
                {
                    var messageActivity = context.Activity as MessageActivity;

                    if (messageActivity.Text == "foo")
                    {
                        context.SendActivityAsync(messageActivity.Text);
                    }

                    if (messageActivity.Text == "NotImplementedException")
                    {
                        throw new NotImplementedException("Test");
                    }

                    return Task.CompletedTask;
                })
                .Send("foo")
                .AssertReply("foo", "passthrough")
                .Send("NotImplementedException")
                .AssertReply("Test")
                .StartTestAsync();
        }
    }
}
