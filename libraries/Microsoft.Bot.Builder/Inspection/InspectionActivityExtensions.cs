// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder
{
    internal static class InspectionActivityExtensions
    {
        public static Activity MakeCommandActivity(this string command)
        {
            return (Activity)Activity.CreateTraceActivity("Command", "https://www.botframework.com/schemas/command", command, "Command");
        }

        public static Activity TraceActivity(this JObject state)
        {
            return (Activity)Activity.CreateTraceActivity("BotState", "https://www.botframework.com/schemas/botState", state, "Bot State");
        }

        public static Activity TraceActivity(this Activity activity, string name, string label)
        {
            return (Activity)Activity.CreateTraceActivity(name, "https://www.botframework.com/schemas/activity", activity, label);
        }

        public static Activity TraceActivity(this ConversationReference conversationReference)
        {
            return (Activity)Activity.CreateTraceActivity("MessageDelete", "https://www.botframework.com/schemas/conversationReference", conversationReference, "Deleted Message");
        }

        public static Activity TraceActivity(this Exception exception)
        {
            return (Activity)Activity.CreateTraceActivity("TurnError", "https://www.botframework.com/schemas/error", exception.Message, "Turn Error");
        }
    }
}
