// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using System.Text;
using System.Web;
using Microsoft.ApplicationInsights.Channel;
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
            var httpContext = HttpContext.Current;
            var request = httpContext?.Request;
            if (request != null && request.HttpMethod == "POST")
            {
                JObject body = null;

                // Retrieve Http Body and pull appropriate ID's.
                try
                {
                    using (var reader = new StreamReader(request.InputStream, Encoding.UTF8, true, 1024, true))
                    {
                        // Set cache options.
                        var bodyAsString = reader.ReadToEnd();
                        body = JObject.Parse(bodyAsString);
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

                var userId = (string)body["from"]?["id"];
                var channelId = (string)body["channelId"];
                var conversationId = (string)body["conversation"]?["id"];

                // Set the user id on the Application Insights telemetry item.
                telemetry.Context.User.Id = channelId + userId;

                // Set the session id on the Application Insights telemetry item.
                telemetry.Context.Session.Id = conversationId;
            }
        }
    }
}
