// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Net;
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
                await ProcessMessageRequestAsync(
                    request,
                    _botFrameworkAdapter,
                    context =>
                    {
                        var bot = httpContext.RequestServices.GetRequiredService<IBot>();

                        return bot.OnReceiveActivity(context);
                    });

                response.StatusCode = (int)HttpStatusCode.OK;
            }
            catch (UnauthorizedAccessException)
            {
                response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        protected abstract Task ProcessMessageRequestAsync(HttpRequest request, BotFrameworkAdapter botFrameworkAdapter, Func<ITurnContext, Task> botCallbackHandler);
    }
}