// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Middleware;

namespace Microsoft.Bot.Builder
{
    public interface IBotContext
    {
        BotAdapter Adapter { get; }

        /// <summary>
        /// Incoming request
        /// </summary>
        Activity Request { get; }

        /// <summary>
        /// Respones
        /// </summary>
        IList<Activity> Responses { get; set; }

        /// <summary>
        /// Conversation reference
        /// </summary>
        ConversationReference ConversationReference { get; }

        /// <summary>
        /// Queues a new "message" responses array.
        /// </summary>
        /// <param name="text">Text of a message to send to the user.</param>
        /// <param name="speak">(Optional) SSML that should be spoken to the user on channels that support speech.</param>
        /// <returns></returns>
        BotContext Reply(string text, string speak = null);

        /// <summary>
        /// Queues a new "message" responses array.
        /// </summary>
        /// <param name="activity">Activity object to send to the user.</param>
        /// <returns></returns>
        BotContext Reply(IActivity activity);

        /// <summary>
        /// Set named service
        /// </summary>
        /// <param name="serviceId"></param>
        /// <param name="service"></param>
        void Set(string serviceId, object service);

        /// <summary>
        /// Get named service
        /// </summary>
        /// <param name="serviceId"></param>
        /// <returns>service</returns>
        object Get(string serviceId);
    }

    public static partial class BotContextExtension
    {
        public static void Set<ServiceT>(this IBotContext context, ServiceT service)
        {
            context.Set(typeof(ServiceT).FullName, service);
        }

        public static ServiceT Get<ServiceT>(this IBotContext context, string serviceId = null)
        {
            return (ServiceT)context.Get(serviceId ?? typeof(ServiceT).FullName);
        }

        //public static BotContext ToBotContext(this IBotContext context)
        //{
        //    return (BotContext)context; 
        //}
    }
}
