// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.LuisV3.Tests
{
    /// <summary>
    /// This is a BotAdapter that does nothing on the Send operation, equivallent to piping to /dev/null.
    /// </summary>
    public class NullAdapter : BotAdapter
    {
        public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken) => throw new NotImplementedException();

        public override Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken) => Task.FromResult(new[] { new ResourceResponse() });

        public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken) => throw new NotImplementedException();
    }
}
