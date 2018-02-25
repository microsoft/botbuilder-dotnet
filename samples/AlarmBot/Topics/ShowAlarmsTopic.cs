// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using AlarmBot.Models;
using AlarmBot.Responses;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Middleware;

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
            var userState = context.GetUserState<UserState>();
            ShowAlarmsResponses.ReplyWithShowAlarms(context, userState.Alarms);
            return Task.CompletedTask;
        }

    }
}
