// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Schema;
using Microsoft.Rest.Serialization;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    internal static class HttpHelper
    {
        public static readonly JsonSerializer BotMessageSerializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            ContractResolver = new ReadOnlyJsonContractResolver(),
            Converters = new List<JsonConverter> { new Iso8601TimeSpanConverter() },
        });

        public static Activity ReadRequest(HttpRequest request)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                var activity = default(Activity);

                using (var bodyReader = new JsonTextReader(new StreamReader(request.Body, Encoding.UTF8)))
                {
                    activity = BotMessageSerializer.Deserialize<Activity>(bodyReader);
                }

                return activity;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public static void WriteResponse(HttpResponse response, InvokeResponse invokeResponse)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            if (invokeResponse == null)
            {
                response.StatusCode = (int)HttpStatusCode.OK;
            }
            else
            {
                response.StatusCode = invokeResponse.Status;

                if (invokeResponse.Body != null)
                {
                    response.ContentType = "application/json";

                    using (var writer = new StreamWriter(response.Body))
                    {
                        using (var jsonWriter = new JsonTextWriter(writer))
                        {
                            BotMessageSerializer.Serialize(jsonWriter, invokeResponse.Body);
                        }
                    }
                }
            }
        }
    }
}
