// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.ContextExtensions
{
    public static class ContextExtensionMethods
    {
        public static async Task<IBotContext> ShowTyping(this IBotContext context)
        {
            Activity activity = ((Activity)context.Request).CreateReply();
            activity.Type = ActivityTypes.Typing;
            await context.Bot.SendActivity(context, new List<IActivity>() { activity });
            return context;
        }

        public static async Task<IBotContext> Delay(this IBotContext context, int duration)
        {
            Activity activity = ((Activity)context.Request).CreateReply();
            activity.Type = ActivityTypes.Delay;
            activity.Value = duration;
            await context.Bot.SendActivity(context, new List<IActivity>() { activity });
            return context;
        }
    }
}