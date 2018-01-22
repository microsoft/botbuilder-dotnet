using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.ContextExtensions
{
    public static class ContextExtensionMethods
    {
        public static IBotContext ShowTyping(this IBotContext context)
        {
            context.Responses.Add(new Connector.Activity { Type = ActivityTypes.Typing });
            return context;
        }

        public static IBotContext Delay(this IBotContext context, int duration)
        {
            context.Responses.Add(new Connector.Activity { Type = "delay", Value = duration });
            return context;
        }
    }
}