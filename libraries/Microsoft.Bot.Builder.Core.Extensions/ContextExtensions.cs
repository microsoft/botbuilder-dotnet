// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Core.Extensions
{
    public static class ContextExtensionMethods
    {
        public static async Task<IBotContext> ShowTyping(this IBotContext context)
        {
            Activity activity = context.Request.CreateReply();
            activity.Type = ActivityTypes.Typing;
            await context.SendActivity(activity);
            return context; 
        }

        public static async Task<IBotContext> Delay(this IBotContext context, int duration)
        {
            Activity activity = context.Request.CreateReply();
            activity.Type = ActivityTypesEx.Delay;
            activity.Value = duration;
            await context.SendActivity(activity);
            return context; 
        }
    }
}
