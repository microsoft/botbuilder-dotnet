// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;

namespace AlarmBot.Responses
{
    public static class DefaultTopicResponses 
    {
        public static void ReplyWithGreeting(IBotContext context)
        {
            context.SendActivity($"Hello, I'm the alarmbot.");
        }
        public static void ReplyWithHelp(IBotContext context)
        {
            context.SendActivity($"I can add an alarm, show alarms or delete an alarm. ");
        }
        public static void ReplyWithResumeTopic(IBotContext context)
        {
            context.SendActivity($"What can I do for you?");
        }
        public static void ReplyWithConfused(IBotContext context)
        {
            context.SendActivity($"I am sorry, I didn't understand that.");
        }
    }
}
