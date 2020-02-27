// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Microsoft.BotBuilderSamples
{
    public class ChildBot : ActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text("child: activity (1)"), cancellationToken);
            await turnContext.SendActivityAsync(MessageFactory.Text("child: activity (2)"), cancellationToken);
            await turnContext.SendActivityAsync(MessageFactory.Text("child: activity (3)"), cancellationToken);
            await turnContext.SendActivityAsync(MessageFactory.Text($"child: {turnContext.Activity.Text}"), cancellationToken);
        }
    }
}
