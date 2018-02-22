// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Samples.Simplified.Asp.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController : BotController
    {
        public MessagesController(BotFrameworkAdapter bot)
            : base(bot)
        {
        }

        protected override Task ReceiveMessage(IBotContext context, IMessageActivity activity)
        {
            long turnNumber = context.State.ConversationProperties["turnNumber"] ?? 0;
            context.State.ConversationProperties["turnNumber"] = ++turnNumber;
            context.Reply($"[{turnNumber}] echo: {activity.Text}");
            return Task.CompletedTask;
        }

        protected override Task ReceiveConversationUpdate(IBotContext context, IConversationUpdateActivity activity)
        {
            foreach (var newMember in activity.MembersAdded)
            {
                if (newMember.Id != activity.Recipient.Id)
                {
                    context.Reply("Hello and welcome to the echo bot.");
                }
            }
            return Task.CompletedTask;
        }
    }
}
