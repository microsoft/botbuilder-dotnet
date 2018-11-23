// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.ApplicationInsights.Core
{
    public class TelemetrySaveBodyASPMiddleware
    {
        private readonly RequestDelegate _next;

        public TelemetrySaveBodyASPMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var request = httpContext?.Request;


            if (request != null && request.Method == "POST")
            {
                var items = httpContext?.Items;
                if (items != null && !items.ContainsKey(TelemetryBotIdInitializer.BotActivityKey))
                {
                    request.EnableBuffering();
                    try
                    {
                        using (var reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
                        {
                            var body = reader.ReadToEnd();
                            var jsonObject = JObject.Parse(body);

                            // Save data in cache.
                            items.Add(TelemetryBotIdInitializer.BotActivityKey, jsonObject);
                        }
                    }
                    catch (JsonReaderException)
                    {
                        // Request not json.
                    }
                    finally
                    {
                        // rewind for next middleware.
                        request.Body.Position = 0;
                    }
                }
            }

            await _next(httpContext);
        }
    }
}
