// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Text;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace AlarmBot
{
    public static class TopicViewHelpers
    {
        public static IMessageActivity ReplyWithSuggestions(IBotContext context, string title, string message, string[] choices)
        {
            var reply = ReplyWithTitle(context, title, message);

            reply.SuggestedActions = new SuggestedActions(
                actions: choices.Select(choice =>
                    new CardAction(type: ActionTypes.ImBack,
                        title: choice,
                        value: choice.ToLower(),
                        displayText: choice.ToLower(),
                        text: choice.ToLower())).ToList());
            return reply;
        }

        public static IMessageActivity ReplyWithTitle(IBotContext context, string title, string message)
        {
            StringBuilder sb = new StringBuilder();
            if (title != null)
                sb.AppendLine($"# {title}\n");

            if (message != null)
                sb.AppendLine(message);

            return ((Activity)context.Request).CreateReply(sb.ToString());
        }


    }
}
