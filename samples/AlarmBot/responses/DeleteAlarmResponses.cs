// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlarmBot.Models;
using Microsoft.Bot.Builder.Core.State;
using Microsoft.Bot.Schema;

namespace AlarmBot.Responses
{
    /// <summary>
    /// I organized all of my output responses as functions so it is easy to reuse and evolve the responses over time without having to rewrite my business logic
    /// </summary>
    public static class DeleteAlarmResponses
    {
        public static async Task ReplyWithNoAlarms(AlarmBotContext context)
        {
            await context.SendActivity($"There are no alarms defined.");
        }

        public static async Task ReplyWithNoAlarmsFound(AlarmBotContext context, string text)
        {
            await context.SendActivity($"There were no alarms found for {(string)text}.");
        }

        public static async Task ReplyWithTitlePrompt(AlarmBotContext context)
        {
            var deleteActivity = GetDeleteActivity(context, (await context.UserState().Get<AlarmUserState>()).Alarms, "Delete Alarms", "What alarm do you want to delete?");
            await context.SendActivity(deleteActivity); 
        }

        public static async Task ReplyWithDeletedAlarm(AlarmBotContext context, Alarm alarm = null)
        {
            await context.SendActivity($"I have deleted {alarm.Title} alarm");
        }

        public static IMessageActivity GetDeleteActivity(AlarmBotContext context, IEnumerable<Alarm> alarms, string title, string message)
        {
            StringBuilder sb = new StringBuilder();
            int i = 1;
            if (alarms.Any())
            {
                foreach (var alarm in alarms)
                    sb.AppendLine($"{i++}. {alarm.Title} {alarm.Time.Value.ToString("f")}");
            }
            else
                sb.AppendLine("There are no alarms defined");
            i = 1;
            return ResponseHelpers.ReplyWithSuggestions(context,
                title,
                $"{message}\n\n{sb.ToString()}",
                alarms.Select(alarm => $"{i++} {alarm.Title}").ToArray());
        }


    }
}
