// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    public static class ContextExtensionMethods
    {
        public static IBotContext ShowTyping(this IBotContext context)
        {
            context.Responses.Add(new Activity { Type = ActivityTypes.Typing });
            return context;
        }

        public static IBotContext Delay(this IBotContext context, int duration)
        {
            context.Responses.Add(new Activity { Type = "delay", Value = duration });
            return context;
        }
    }
}
