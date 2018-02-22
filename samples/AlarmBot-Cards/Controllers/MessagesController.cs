// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AlarmBot.Models;
using AlarmBot.Topics;
using AlarmBot.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Builder.Storage;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace AlarmBot.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController : BotController
    {
        public MessagesController(BotFrameworkAdapter adapter) : base(adapter)
        {
        }

        public override async Task OnReceiveActivity(IBotContext context)
        {
            // --- Bot logic 
            bool handled = false;
            // Get the current ActiveTopic from my persisted conversation state
            var activeTopic = context.State.ConversationProperties[ConversationProperties.ACTIVETOPIC] as ITopic;
            // if we don't have an active topic yet
            if (activeTopic == null)
            {
                // use the default topic
                activeTopic = new DefaultTopic();
                context.State.ConversationProperties[ConversationProperties.ACTIVETOPIC] = activeTopic;
                handled = await activeTopic.StartTopic(context);
            }
            else
            {
                // we do have an active topic, so call it 
                handled = await activeTopic.ContinueTopic(context);
            }
            
            // if activeTopic's result is false and the activeTopic is NOT already the default topic
            if (handled == false && !(activeTopic is DefaultTopic))
            {
                // USe DefaultTopic as the active topic
                context.State.ConversationProperties[ConversationProperties.ACTIVETOPIC] = new DefaultTopic();
                handled = await activeTopic.ResumeTopic(context);
            }
        }
    }
}
