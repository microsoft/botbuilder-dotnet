// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AlarmBot.Models;
using AlarmBot.Topics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Middleware;
using System.Threading.Tasks;

namespace AlarmBot.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController : BotController
    {
        public MessagesController(BotFrameworkAdapter adapter) : base(adapter) { }

        protected override async Task OnReceiveActivity(IBotContext context)
        {
            // --- Our receive handler simply inspects the persisted ITopic class and calls to it as appropriate ---

            bool handled = false;
            // Get the current ActiveTopic from my persisted conversation state
            var conversationState = context.GetConversationState<AlarmConversationState>();

            // if we don't have an active topic yet
            if (conversationState.ActiveTopic== null)
            {
                // use the default topic
                conversationState.ActiveTopic = new DefaultTopic();
                handled = await conversationState.ActiveTopic.StartTopic(context);
            }
            else
            {
                // we do have an active topic, so call it 
                handled = await conversationState.ActiveTopic.ContinueTopic(context);
            }

            // if activeTopic's result is false and the activeTopic is NOT already the default topic
            if (handled == false && !(conversationState.ActiveTopic is DefaultTopic))
            {
                // USe DefaultTopic as the active topic
                conversationState.ActiveTopic = new DefaultTopic();
                handled = await conversationState.ActiveTopic.ResumeTopic(context);
            }
        }
    }
}
