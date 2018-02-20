// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using AlarmBot.Models;
using AlarmBot.TopicViews;
using Microsoft.Bot.Builder;

namespace AlarmBot.Topics
{
    /// <summary>
    /// Topic for showing alarms
    /// </summary>
    public class ShowAlarmsTopic : BaseTopic
    {
        public ShowAlarmsTopic()
        {
            this.Name = "ShowAlarms";
        }

        /// <summary>
        /// Called when topic is activated (SINGLE TURN)
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<bool> StartTopic(IBotContext context)
        {
            await ShowAlarms(context);

            // end the topic immediately
            return false;
        }

        public static Task ShowAlarms(IBotContext context)
        {
            List<Alarm> alarms = GetAlarms(context);
            context.ReplyWith(ShowAlarmsTopicView.SHOWALARMS, alarms);
            return Task.CompletedTask;
        }

        public static List<Alarm> GetAlarms(IBotContext context)
        {
            var alarms = (List<Alarm>)context.State.UserProperties[UserProperties.ALARMS];
            if (alarms == null)
            {
                alarms = new List<Alarm>();
                context.State.UserProperties[UserProperties.ALARMS] = alarms;
            }

            return alarms;
        }
    }
}
