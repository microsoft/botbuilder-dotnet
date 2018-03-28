// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder;

namespace AlarmBot.Responses
{
    public static class DefaultTopicResponses 
    {
        public static async Task ReplyWithGreeting(ITurnContext context)
        {
            await context.SendActivity($"Hello, I'm the alarmbot.");
        }
        public static async Task ReplyWithHelp(ITurnContext context)
        {
            await context.SendActivity($"I can add an alarm, show alarms or delete an alarm. ");
        }
        public static async Task ReplyWithResumeTopic(ITurnContext context)
        {
            await context.SendActivity($"What can I do for you?");
        }
        public static async Task ReplyWithConfused(ITurnContext context)
        {
            await context.SendActivity($"I am sorry, I didn't understand that.");
        }
    }
}
