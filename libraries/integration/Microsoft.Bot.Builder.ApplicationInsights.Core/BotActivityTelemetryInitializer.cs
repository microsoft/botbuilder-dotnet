// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Microsoft.Bot.Builder.ApplicationInsights.Core
{
    /// <summary>
    /// A telemetry initializer that records various details of each <see cref="Activity"/> that flows through a turn.
    /// </summary>
    /// <seealso cref="Activity"/>
    /// <seealso cref="ITurnContext"/>
    public sealed class BotActivityTelemetryInitializer : ITelemetryInitializer
    {
        public BotActivityTelemetryInitializer()
        {
        }

        public void Initialize(ITelemetry telemetry)
        {
            if (telemetry is RequestTelemetry)
            {
                var activity = ActivityTelemetryMiddleware.GetActivityForCurrentTurn();

                if (activity != null)
                {
                    var telemetryContext = telemetry.Context;

                    // Set the user id on the Application Insights telemetry item.
                    telemetryContext.User.Id = activity.From.Id;

                    // Set the session id on the Application Insights telemetry item.
                    telemetryContext.Session.Id = activity.Conversation.Id;

                    var globalProperties = telemetryContext.GlobalProperties;
                    globalProperties.Add("BotFramework.ChannelId", activity.ChannelId);
                }
            }
        }
    }
}
