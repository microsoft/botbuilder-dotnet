// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Rest.Serialization;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    /// <summary>
    /// Helper class with methods to help with reading and responding to http requests.
    /// </summary>
    public static class HttpHelper
    {
        /// <summary>
        /// An instance of <see cref="JsonSerializerSettings"/> used by the <see cref="ChannelServiceController"/>.
        /// </summary>
        public static readonly JsonSerializerSettings BotMessageSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            ContractResolver = new ReadOnlyJsonContractResolver(),
            Converters = new List<JsonConverter> { new Iso8601TimeSpanConverter() }
        };

        /// <summary>
        /// An instance of <see cref="JsonSerializer"/> created using <see cref="BotMessageSerializerSettings"/>.
        /// </summary>
        public static readonly JsonSerializer BotMessageSerializer = JsonSerializer.Create(BotMessageSerializerSettings);

        /// <summary>
        /// Accepts an incoming HttpRequest and deserializes it using the <see cref="BotMessageSerializer"/>.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the request into.</typeparam>
        /// <param name="request">The HttpRequest.</param>
        /// <returns>The deserialized request.</returns>
        public static async Task<T> ReadRequestAsync<T>(HttpRequest request)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                using (var memoryStream = new MemoryStream())
                {
                    await request.Body.CopyToAsync(memoryStream).ConfigureAwait(false);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    using (var bodyReader = new JsonTextReader(new StreamReader(memoryStream, Encoding.UTF8)))
                    {
                        return BotMessageSerializer.Deserialize<T>(bodyReader);
                    }
                }
            }
            catch (JsonException)
            {
                return default;
            }
        }

        /// <summary>
        /// If an <see cref="InvokeResponse"/> is provided the status and body of the <see cref="InvokeResponse"/>
        /// are used to set the status and body of the <see cref="HttpResponse"/>. If no <see cref="InvokeResponse"/>
        /// is provided then the status of the <see cref="HttpResponse"/> is set to 200.
        /// </summary>
        /// <param name="response">A HttpResponse.</param>
        /// <param name="invokeResponse">An instance of <see cref="InvokeResponse"/>.</param>
        /// <returns>A Task representing the work to be executed.</returns>
        public static async Task WriteResponseAsync(HttpResponse response, InvokeResponse invokeResponse)
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

                    using (var memoryStream = new MemoryStream())
                    {
                        using (var writer = new StreamWriter(memoryStream, new UTF8Encoding(false, false), 1024, true))
                        {
                            using (var jsonWriter = new JsonTextWriter(writer))
                            {
                                BotMessageSerializer.Serialize(jsonWriter, invokeResponse.Body);
                            }
                        }

                        memoryStream.Seek(0, SeekOrigin.Begin);
                        await memoryStream.CopyToAsync(response.Body).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
