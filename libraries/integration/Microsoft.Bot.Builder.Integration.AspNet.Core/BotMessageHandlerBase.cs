// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Microsoft.Rest.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Handlers
{
    public abstract class BotMessageHandlerBase
    {
        public static readonly JsonSerializer BotMessageSerializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Newtonsoft.Json.Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        });

        private BotFrameworkAdapter _botFrameworkAdapter;

        public BotMessageHandlerBase(BotFrameworkAdapter botFrameworkAdapter)
        {
            _botFrameworkAdapter = botFrameworkAdapter;
        }

        public async Task HandleAsync(HttpContext httpContext)
        {
            var request = httpContext.Request;
            var response = httpContext.Response;

            if (request.Method != HttpMethods.Post)
            {
                response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;

                return;
            }

            if (request.ContentLength == 0)
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;

                return;
            }

            if (!MediaTypeHeaderValue.TryParse(request.ContentType, out var mediaTypeHeaderValue)
                    ||
                mediaTypeHeaderValue.MediaType != "application/json")
            {
                response.StatusCode = (int)HttpStatusCode.NotAcceptable;

                return;
            }

            try
            {
                var invokeResponse = await ProcessMessageRequestAsync(
                    request,
                    _botFrameworkAdapter,
                    context =>
                    {
                        var bot = httpContext.RequestServices.GetRequiredService<IBot>();

                        return bot.OnTurn(context);
                    });

                if (invokeResponse == null)
                {
                    response.StatusCode = (int)HttpStatusCode.OK;
                }
                else
                {
                    // In the event that an InvokeRepsonse is returned, it's up to us to take the status
                    // code and Body from that object and return them. 

                    // Taken from the ClientConnector.cs setting for JSON serialization. 
                    var serializationSettings = new JsonSerializerSettings
                    {
                        Formatting = Newtonsoft.Json.Formatting.Indented,
                        DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat,
                        DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc,
                        NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                        ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Serialize,
                        ContractResolver = new ReadOnlyJsonContractResolver(),
                        Converters = new List<JsonConverter>
                        {
                            new Iso8601TimeSpanConverter()
                        }
                    };
                    
                    string bodyContent = Rest.Serialization.SafeJsonConvert.SerializeObject(invokeResponse.Body, serializationSettings);
                    byte[] data = Encoding.UTF8.GetBytes(bodyContent);

                    response.ContentType = "application/json";
                    await response.Body.WriteAsync(data, 0, data.Length);
                    response.StatusCode = invokeResponse.Status;
                }
            }
            catch (UnauthorizedAccessException)
            {
                response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        protected abstract Task<InvokeResponse> ProcessMessageRequestAsync(HttpRequest request, BotFrameworkAdapter botFrameworkAdapter, Func<ITurnContext, Task> botCallbackHandler);
    }
}