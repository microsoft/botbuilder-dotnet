// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using AlarmBot.Models;
using Microsoft.Bot.Builder;

namespace AlarmBot
{
    public interface ITopic 
    {
        /// <summary>
        /// Name of the topic
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Called when topic starts
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<bool> StartTopic(AlarmBotContext context);

        /// <summary>
        /// called while topic active
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<bool> ContinueTopic(AlarmBotContext context);

        /// <summary>
        ///  Called when a topic is resumed
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<bool> ResumeTopic(AlarmBotContext context);
    }
}
