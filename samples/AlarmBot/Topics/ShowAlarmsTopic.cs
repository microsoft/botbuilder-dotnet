// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using AlarmBot.Models;
using AlarmBot.Responses;
using Microsoft.Bot.Builder;

namespace AlarmBot.Topics
{
    /// <summary>
    /// Class around topic of listing alarms
    /// </summary>
    public class ShowAlarmsTopic : ITopic
    {
        public ShowAlarmsTopic()
        {
        }

        public string Name { get; set;  } = "ShowAlarms";

        /// <summary>
        /// Called when topic is activated (SINGLE TURN)
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<bool> StartTopic(IBotContext context)
        {
            await ShowAlarms(context);

            // end the topic immediately
            return false;
        }

        public Task<bool> ContinueTopic(IBotContext context)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> ResumeTopic(IBotContext context)
        {
            throw new System.NotImplementedException();
        }


        public static Task ShowAlarms(IBotContext context)
        {
            List<Alarm> alarms = GetAlarms(context);
            ShowAlarmsResponses.ReplyWithShowAlarms(context, alarms);
            return Task.CompletedTask;
        }

        public static List<Alarm> GetAlarms(IBotContext context)
        {
            var alarms = (List<Alarm>)context.State.User[UserProperties.ALARMS];
            if (alarms == null)
            {
                alarms = new List<Alarm>();
                context.State.User[UserProperties.ALARMS] = alarms;
            }

            return alarms;
        }
    }
}
