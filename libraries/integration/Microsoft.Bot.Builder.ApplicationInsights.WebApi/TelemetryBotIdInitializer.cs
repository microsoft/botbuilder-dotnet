// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using System.Text;
using System.Web;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.ApplicationInsights.WebApi
{
    /// <summary>
    /// Initializer that sets the user ID based on Bot data.
    /// </summary>
    public class TelemetryBotIdInitializer : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            if (telemetry == null)
            {
                return;
            }

            var httpContext = HttpContext.Current;
            var items = httpContext?.Items;

            if (items != null)
            {
                if (!HttpContext.Current.Items.Contains("BotActivity"))
                {
                    CacheBody();
                }

                if ((telemetry is RequestTelemetry || telemetry is EventTelemetry) && HttpContext.Current.Items.Contains("BotActivity"))
                {
                    var body = items["BotActivity"] as JObject;
                    if (body != null)
                    {
                        var userId = (string)body["from"]?["id"];
                        var channelId = (string)body["channelId"];
                        var conversationId = (string)body["conversation"]?["id"];

                        // Set the user id on the Application Insights telemetry item.
                        telemetry.Context.User.Id = channelId + userId;

                        // Set the session id on the Application Insights telemetry item.
                        telemetry.Context.Session.Id = conversationId;

                        // Set the activity id https://github.com/Microsoft/botframework-obi/blob/master/botframework-activity/botframework-activity.md#id
                        telemetry.Context.GlobalProperties.Add("activityId", (string)body["id"]);
                        // Set the channel id https://github.com/Microsoft/botframework-obi/blob/master/botframework-activity/botframework-activity.md#channel-id
                        telemetry.Context.GlobalProperties.Add("channelId", (string)body["channelId "]);
                        // Set the activity type https://github.com/Microsoft/botframework-obi/blob/master/botframework-activity/botframework-activity.md#type
                        telemetry.Context.GlobalProperties.Add("activityType", (string)body["type"]);
                    }
                }
            }
        }

        private void CacheBody()
        {
            var httpContext = HttpContext.Current;
            var request = httpContext?.Request;
            if (request != null && request.HttpMethod == "POST")
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
                        httpContext.Items["BotActivity"] = body;
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
