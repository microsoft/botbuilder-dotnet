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
        IBotContext Reply(string text, string speak = null);

        /// <summary>
        /// Queues a new "message" responses array.
        /// </summary>
        /// <param name="activity">Activity object to send to the user.</param>
        /// <returns></returns>
        IBotContext Reply(IActivity activity);

        /// <summary>
        /// Set object by Id
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="object"></param>
        void Set(string objectId, object @object);

        /// <summary>
        /// Get object by id
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns>service</returns>
        object Get(string objectId);
    }

    public static partial class BotContextExtension
    {
        public static void Set<ObjectT>(this IBotContext context, ObjectT service)
        {
            var objectId = $"{typeof(ObjectT).Namespace}.{typeof(ObjectT).Name}";
            context.Set(objectId, service);
        }

        public static ObjectT Get<ObjectT>(this IBotContext context, string objectId = null)
        {
            if (objectId == null)
                objectId = $"{typeof(ObjectT).Namespace}.{typeof(ObjectT).Name}";
            return (ObjectT)context.Get(objectId);
        }

    }
}
