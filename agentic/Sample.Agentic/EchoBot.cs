// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Sample.Agentic;

public class EchoBot : ActivityHandler
{
    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
    {
        var replyText = $"You said: {turnContext.Activity.Text}";

        await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
    }
}
