// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlarmBot.Models;
using Microsoft.Bot.Builder;

namespace AlarmBot.Responses
{
    /// <summary>
    /// I organized all of my output responses as functions so it is easy to reuse and evolve the responses over time without having to rewrite my business logic
    /// </summary>
    public static class ShowAlarmsResponses
    {
        public static async Task ReplyWithShowAlarms(ITurnContext context, IEnumerable<Alarm> alarms)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Current Alarms\n");

            if (alarms != null && alarms.Any())
            {
                foreach (var alarm in alarms)
                {
                    sb.AppendLine($"* Title: {alarm.Title} Time: {alarm.Time.Value.ToString("f")}");
                }
            }
            else
                sb.AppendLine("*There are no alarms defined.*");

            await context.SendActivity(sb.ToString());
        }
    }
}
