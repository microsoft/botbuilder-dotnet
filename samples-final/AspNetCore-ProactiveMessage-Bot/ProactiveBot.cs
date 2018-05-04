// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace AspNetCore_ProactiveMessage_Bot
{
    public class ProactiveBot : IBot
    {
        public async Task OnTurn(ITurnContext context)
        {
            var state = context.GetConversationState<ProactiveState>();
            switch (context.Activity.Type)
            {
                case ActivityTypes.ConversationUpdate:
                    if (context.Activity.MembersAdded.FirstOrDefault()?.Id == context.Activity.From.Id)
                    {
                        // initial message with instructions to reset the count
                        var conversation = TurnContext.GetConversationReference(context.Activity);
                        conversation.Conversation.Role = "Proactive";
                        await context.SendActivity("Hello! This is a proactive message sample");
                        await context.SendActivity($"To reset the counter used in this conversation make a POST with the follwing content");
                        await context.SendActivity(JsonConvert.SerializeObject(conversation));
                    }
                    break;
                case ActivityTypes.Message:
                    if (IsProactiveMessage(context))
                    {
                        // handle proative message
                        state.Count = 0;
                        await context.SendActivity("Reset counter!");
                    }
                    else
                    {
                        // message from user
                        await context.SendActivity($"Echo {++state.Count}: \"{context.Activity.Text}\"");
                    }
                    break;
            }
        }

        private bool IsProactiveMessage(ITurnContext context)
        {
            return context.Activity.Conversation.Role == "Proactive" || context.Activity.Text == null;
        }
    }
}