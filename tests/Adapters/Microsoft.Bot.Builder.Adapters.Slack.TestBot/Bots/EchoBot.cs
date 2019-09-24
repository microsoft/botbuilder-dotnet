// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.3.0

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Adapters.Slack.TestBot.Bots
{
    /// <summary>
    /// An EchoBot class that extends from the ActivityHandler.
    /// </summary>
    public class EchoBot : ActivityHandler
    {
        /// <summary>
        /// OnMessageActivityAsync method that returns an async Task.
        /// </summary>
        /// <param name="turnContext">turnContext of ITurnContext{T}, where T is an IActivity.</param>
        /// <param name="cancellationToken">cancellationToken propagates notifications that operations should be canceled.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text($"Echo: {turnContext.Activity.Text}"), cancellationToken);
        }

        /// <summary>
        /// OnMessageActivityAsync method that returns an async Task.
        /// </summary>
        /// <param name="turnContext">turnContext of ITurnContext{T}, where T is an IActivity.</param>
        /// <param name="cancellationToken">cancellationToken propagates notifications that operations should be canceled.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        protected override async Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            System.Diagnostics.Debug.WriteLine($"You sent: /test");
        }

        /// <summary>
        /// OnMembersAddedAsync method that returns an async Task.
        /// </summary>
        /// <param name="membersAdded">membersAdded of IList{T}, where T is ChannelAccount.</param>
        /// <param name="turnContext">turnContext of ITurnContext{T}, where T is an IConversationUpdateActivity.</param>
        /// <param name="cancellationToken">cancellationToken propagates notifications that operations should be canceled.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hello and Welcome!"), cancellationToken);
                }
            }
        }
    }
}
