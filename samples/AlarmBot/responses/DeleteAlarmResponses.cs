// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlarmBot.Models;
using AlarmBot.Topics;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Schema;

namespace AlarmBot.Responses
{
    /// <summary>
    /// I organized all of my output responses as functions so it is easy to reuse and evolve the responses over time without having to rewrite my business logic
    /// </summary>
    public static class DeleteAlarmResponses
    {
        public static void ReplyWithNoAlarms(AlarmBotContext context)
        {
            context.Reply($"There are no alarms defined.");
        }

        public static void ReplyWithNoAlarmsFound(AlarmBotContext context, string text)
        {
            context.Reply($"There were no alarms found for {(string)text}.");
        }

        public static void ReplyWithTitlePrompt(AlarmBotContext context)
        {
            context.Reply(GetDeleteActivity(context, context.UserState.Alarms, "Delete Alarms", "What alarm do you want to delete?"));
        }

        public static void ReplyWithDeletedAlarm(AlarmBotContext context, Alarm alarm = null)
        {
            context.Reply($"I have deleted {alarm.Title} alarm");
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
