// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public static class ContextExtensionMethods
    {
        public static IBotContext ShowTyping(this IBotContext context)
        {
            Activity activity = ((Activity)context.Request).CreateReply();
            activity.Type = ActivityTypes.Typing;
            return context.Reply((IActivity)activity);
        }

        public static IBotContext Delay(this IBotContext context, int duration)
        {
            Activity activity = ((Activity)context.Request).CreateReply();
            activity.Type = ActivityTypesEx.Delay;
            activity.Value = duration;
            return context.Reply(activity);
        }
    }
}
