// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlarmBot.Models;
using AlarmBot.Responses;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;

namespace AlarmBot.Topics
{
    /// <summary>
    /// Topic for showing alarms
    /// </summary>
    public class ShowAlarmsTopic : ITopic
    {
        public string Name { get; set; } = "ShowAlarms";

        /// <summary>
        /// Called when topic is activated (SINGLE TURN)
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<bool> StartTopic(ITurnContext context)
        {
            await ShowAlarms(context);

            // end the topic immediately
            return false;
        }

        /// <summary>
        /// called while topic active
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task<bool> ContinueTopic(ITurnContext context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///  Called when a topic is resumed
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task<bool> ResumeTopic(ITurnContext context)
        {
            throw new NotImplementedException();
        }


        public static async Task ShowAlarms(ITurnContext context)
        {
            var userState = context.GetUserState<UserData>();

            if (userState.Alarms == null)
            {
                userState.Alarms = new List<Alarm>();
            }

            await ShowAlarmsTopicResponses.ReplyWithShowAlarms(context, userState.Alarms);            
        }
    }
}
