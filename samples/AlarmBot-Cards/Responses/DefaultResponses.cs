// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder;

namespace AlarmBot.Responses
{
    public static class DefaultTopicResponses 
    {
        public static void ReplyWithGreeting(IBotContext context)
        {
            context.Reply($"Hello, I'm the alarmbot.");
        }
        public static void ReplyWithHelp(IBotContext context)
        {
            context.Reply($"I can add an alarm, show alarms or delete an alarm. ");
        }
        public static void ReplyWithResumeTopic(IBotContext context)
        {
            context.Reply($"What can I do for you?");
        }
        public static void ReplyWithConfused(IBotContext context)
        {
            context.Reply($"I am sorry, I didn't understand that.");
        }

    }
}
