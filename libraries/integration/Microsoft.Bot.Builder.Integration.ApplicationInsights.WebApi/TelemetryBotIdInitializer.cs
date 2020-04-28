// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Integration.ApplicationInsights.WebApi
{
    /// <summary>
    /// Initializer that sets the user ID and session ID (in addition to other bot-specific properties such as activity ID).
    /// </summary>
    public class TelemetryBotIdInitializer : ITelemetryInitializer
    {
        public static readonly string BotActivityKey = "BotBuilderActivity";

        public void Initialize(ITelemetry telemetry)
        {
            var httpContext = HttpContext.Current;
            var items = httpContext?.Items;

            if (items != null)
            {
                CacheBody();

                if (telemetry is RequestTelemetry || telemetry is EventTelemetry
                    || telemetry is TraceTelemetry || telemetry is DependencyTelemetry)
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

                        var context = telemetry.Context;

                        // Set the user id on the Application Insights telemetry item.
                        context.User.Id = channelId + userId;

                        // Set the session id on the Application Insights telemetry item using hashed conversation Id.
                        // Hashed ID is used due to max session ID length for App Insights session Id
                        context.Session.Id = sessionId;

                        var telemetryProperties = ((ISupportProperties)telemetry).Properties;

                        // Set the conversation id
                        telemetryProperties.Add("conversationId", conversationId);

                        // Set the activity id https://github.com/microsoft/botframework-obi/blob/master/protocols/botframework-activity/botframework-activity.md#id
                        telemetryProperties.Add("activityId", (string)body["id"]);

                        // Set the channel id https://github.com/microsoft/botframework-obi/blob/master/protocols/botframework-activity/botframework-activity.md#channel-id
                        telemetryProperties.Add("channelId", (string)channelId);

                        // Set the activity type https://github.com/microsoft/botframework-obi/blob/master/protocols/botframework-activity/botframework-activity.md#type
                        telemetryProperties.Add("activityType", (string)body["type"]);
                    }
                }
            }
        }

        private void CacheBody()
        {
            var httpContext = HttpContext.Current;
            var request = httpContext.Request;
            if (request.HttpMethod == "POST" && request.ContentType == "application/json")
            {
                JObject body = null;

                // Retrieve Http Body and cache body.
                try
                {
                    using (var reader = new StreamReader(request.InputStream, Encoding.UTF8, true, 1024, true))
                    {
                        // Set cache options.
                        var bodyAsString = reader.ReadToEnd();
                        body = JObject.Parse(bodyAsString);
                        httpContext.Items[BotActivityKey] = body;
                    }
                }
                catch (JsonReaderException)
                {
                    // Request not json.
                    return;
                }
                finally
                {
                    // rewind for next middleware.
                    request.InputStream.Position = 0;
                }
            }
        }
    }
}
