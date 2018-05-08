// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Rest.Serialization;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Integration.AspNet.WebApi.Handlers
{
    public abstract class BotMessageHandlerBase : HttpMessageHandler
    {
        public static readonly MediaTypeFormatter[] BotMessageMediaTypeFormatters = new[] {
            new JsonMediaTypeFormatter
            {
                SerializerSettings =
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented,
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                    ContractResolver = new ReadOnlyJsonContractResolver(),
                    Converters = new List<JsonConverter> { new Iso8601TimeSpanConverter() }
                }
            }
        };

        private readonly BotFrameworkAdapter _botFrameworkAdapter;

        public BotMessageHandlerBase(BotFrameworkAdapter botFrameworkAdapter)
        {
            _botFrameworkAdapter = botFrameworkAdapter ?? throw new ArgumentNullException(nameof(botFrameworkAdapter));
        }

        internal BotFrameworkAdapter BotFrameworkAdapter => _botFrameworkAdapter;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method != HttpMethod.Post)
            {
                return request.CreateResponse(HttpStatusCode.MethodNotAllowed);
            }

            var requestContentHeaders = request.Content.Headers;

            if (requestContentHeaders.ContentLength == 0)
            {
                return request.CreateErrorResponse(HttpStatusCode.BadRequest, "Request body should not be empty.");
            }

            if (!BotMessageMediaTypeFormatters[0].SupportedMediaTypes.Contains(requestContentHeaders.ContentType))
            {
                return request.CreateErrorResponse(HttpStatusCode.NotAcceptable, $"Expecting Content-Type of \"{BotMessageMediaTypeFormatters[0].SupportedMediaTypes[0].MediaType}\".");
            }

            try
            {
                var invokeResponse = await ProcessMessageRequestAsync(
                    request,
                    _botFrameworkAdapter,
                    context =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        IBot bot;

                        try
                        {
                            bot = (IBot)request.GetDependencyScope().GetService(typeof(IBot));
                        }
                        catch
                        {
                            bot = null;
                        }

                        if (bot == null)
                        {
                            throw new InvalidOperationException($"Did not find an {typeof(IBot).Name} service via the dependency resolver. Please make sure you have registered your bot with your dependency injection container.");
                        }

                        return bot.OnTurn(context);
                    },
                    cancellationToken);

                if (invokeResponse == null)
                {
                    return request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    var response = request.CreateResponse((HttpStatusCode)invokeResponse.Status);
                    response.Content = new ObjectContent(
                        invokeResponse.Body.GetType(),
                        invokeResponse.Body,
                        BotMessageMediaTypeFormatters[0]);

                    return response;
                }
            }
            catch (UnauthorizedAccessException e)
            {
                return request.CreateErrorResponse(HttpStatusCode.Unauthorized, e.Message);
            }
            catch (InvalidOperationException e)
            {
                return request.CreateErrorResponse(HttpStatusCode.NotFound, e.Message);
            }
        }

        protected abstract Task<InvokeResponse> ProcessMessageRequestAsync(HttpRequestMessage request, BotFrameworkAdapter botFrameworkAdapter, Func<ITurnContext, Task> botCallbackHandler, CancellationToken cancellationToken);
    }
}