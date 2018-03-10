// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlarmBot.Models;
using AlarmBot.Responses;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace AlarmBot.Topics
{
    /// <summary>
    /// Class around topic of deleting an alarm
    /// </summary>
    public class DeleteAlarmTopic : ITopic
    {

        public DeleteAlarmTopic()
        {
        }

        public string Name { get; set; } = "DeleteAlarm";

        /// <summary>
        /// The alarm title we are searching for
        /// </summary>
        public string AlarmTitle { get; set; }

        /// <summary>
        /// Called when the topic is started
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task<bool> StartTopic(AlarmBotContext context)
        {
            this.AlarmTitle = context.RecognizedIntents.TopIntent.Entities.Where(entity => entity.GroupName == "AlarmTitle")
                                .Select(entity => entity.ValueAs<string>()).FirstOrDefault();

            return FindAlarm(context);
        }

        /// <summary>
        /// Called for every turn while active
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<bool> ContinueTopic(AlarmBotContext context)
        {
            if (context.Request.Type == ActivityTypes.Message)
            {
                this.AlarmTitle = context.Request.AsMessageActivity().Text.Trim();
                return await this.FindAlarm(context);
            }
            return true;
        }

        /// <summary>
        /// Called when resuming from an interruption
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task<bool> ResumeTopic(AlarmBotContext context)
        {
            return this.FindAlarm(context);
        }

        public async Task<bool> FindAlarm(AlarmBotContext context)
        {
            if (context.UserState.Alarms == null)
            {
                context.UserState.Alarms = new List<Alarm>();
            }

            // Ensure there are context.UserState.Alarms to delete
            if (context.UserState.Alarms.Count == 0)
            {
                DeleteAlarmResponses.ReplyWithNoAlarms(context);
                return false;
            }

            // Validate title
            if (!String.IsNullOrWhiteSpace(this.AlarmTitle))
            {
                if (int.TryParse(this.AlarmTitle.Split(' ').FirstOrDefault(), out int index))
                {
                    if (index > 0 && index <= context.UserState.Alarms.Count)
                    {
                        index--;
                        // Delete selected alarm and end topic
                        var alarm = context.UserState.Alarms.Skip(index).First();
                        context.UserState.Alarms.Remove(alarm);
                        DeleteAlarmResponses.ReplyWithDeletedAlarm(context, alarm);
                        return false; // cancel topic
                    }
                }
                else
                {
                    var parts = this.AlarmTitle.Split(' ');
                    var choices = context.UserState.Alarms.Where(alarm => parts.Any(part => alarm.Title.Contains(part))).ToList();

                    if (choices.Count == 0)
                    {
                        DeleteAlarmResponses.ReplyWithNoAlarmsFound(context, this.AlarmTitle);
                        return false;
                    }
                    else if (choices.Count == 1)
                    {
                        // Delete selected alarm and end topic
                        var alarm = choices.First();
                        context.UserState.Alarms.Remove(alarm);
                        DeleteAlarmResponses.ReplyWithDeletedAlarm(context, alarm);
                        return false; // cancel topic
                    }
                }
            }

            // Prompt for title
            DeleteAlarmResponses.ReplyWithTitlePrompt(context);
            return true;
        }
    }
}
