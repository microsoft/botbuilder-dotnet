// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Integration.ApplicationInsights.Core
{
    /// <summary>
    /// Initializer that sets the user ID and session ID (in addition to other bot-specific properties such as activity ID).
    /// </summary>
    public class TelemetryBotIdInitializer : ITelemetryInitializer
    {
        public static readonly string BotActivityKey = "BotBuilderActivity";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TelemetryBotIdInitializer(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public void Initialize(ITelemetry telemetry)
        {
            if (telemetry == null)
            {
                return;
            }

            var httpContext = _httpContextAccessor.HttpContext;
            var items = httpContext?.Items;

            if (items != null)
            {
                if ((telemetry is RequestTelemetry || telemetry is EventTelemetry
                    || telemetry is TraceTelemetry || telemetry is DependencyTelemetry || telemetry is PageViewTelemetry)
                    && items.ContainsKey(BotActivityKey))
                {
                    if (items[BotActivityKey] is JObject body)
                    {
                        var userId = string.Empty;
                        var from = body["from"];
                        if (!string.IsNullOrWhiteSpace(from?.ToString()))
                        {
                            userId = (string)from["id"];
                        }

                        var channelId = (string)body["channelId"];

                        var conversationId = string.Empty;
                        var sessionId = string.Empty;
                        var conversation = body["conversation"];
                        if (!string.IsNullOrWhiteSpace(conversation?.ToString()))
                        {
                            conversationId = (string)conversation["id"];

                            sessionId = StringUtils.Hash(conversationId);
                        }

                        // Set the user id on the Application Insights telemetry item.
                        telemetry.Context.User.Id = channelId + userId;

                        // Set the session id on the Application Insights telemetry item.
                        // Hashed ID is used due to max session ID length for App Insights session Id
                        telemetry.Context.Session.Id = sessionId;

                        var telemetryProperties = ((ISupportProperties)telemetry).Properties;

                        // Set the conversation id
                        if (!telemetryProperties.ContainsKey("conversationId"))
                        {
                            telemetryProperties.Add("conversationId", conversationId);
                        }

                        // Set the activity id https://github.com/Microsoft/botframework-obi/blob/master/botframework-activity/botframework-activity.md#id
                        if (!telemetryProperties.ContainsKey("activityId"))
                        {
                            telemetryProperties.Add("activityId", (string)body["id"]);
                        }

                        // Set the channel id https://github.com/Microsoft/botframework-obi/blob/master/botframework-activity/botframework-activity.md#channel-id
                        if (!telemetryProperties.ContainsKey("channelId"))
                        {
                            telemetryProperties.Add("channelId", (string)channelId);
                        }

                        // Set the activity type https://github.com/Microsoft/botframework-obi/blob/master/botframework-activity/botframework-activity.md#type
                        if (!telemetryProperties.ContainsKey("activityType"))
                        {
                            telemetryProperties.Add("activityType", (string)body["type"]);
                        }
                    }
                }
            }
        }
    }
}
