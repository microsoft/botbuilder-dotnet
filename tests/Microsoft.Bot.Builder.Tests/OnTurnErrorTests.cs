// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Xunit;

namespace Microsoft.Bot.Builder.Tests
{
    public class OnTurnErrorTests
    {
        [Fact]
        public async Task OnTurnError_Test()
        {
            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation("OnTurnError_Test"));
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

            await new TestFlow(adapter, (context, cancellationToken) =>
                {
                    if (context.Activity.AsMessageActivity().Text == "foo")
                    {
                        context.SendActivityAsync(context.Activity.AsMessageActivity().Text);
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
                .StartTestAsync();
        }
    }
}
