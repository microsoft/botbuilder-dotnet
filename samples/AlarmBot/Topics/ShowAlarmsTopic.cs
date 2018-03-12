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
        public async Task<bool> StartTopic(AlarmBotContext context)
        {
            await ShowAlarms(context);

            // end the topic immediately
            return false;
        }

        public Task<bool> ContinueTopic(AlarmBotContext context)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> ResumeTopic(AlarmBotContext context)
        {
            throw new System.NotImplementedException();
        }


        public static Task ShowAlarms(AlarmBotContext context)
        {
            ShowAlarmsResponses.ReplyWithShowAlarms(context, context.UserState.Alarms);
            return Task.CompletedTask;
        }

    }
}
