// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Integration.AspNet.WebApi.Handlers
{
    public abstract class BotMessageHandlerBase : HttpMessageHandler
    {
        public static readonly MediaTypeFormatter[] BotMessageMediaTypeFormatters =
        {
            new JsonMediaTypeFormatter
            {
                SerializerSettings = MessageSerializerSettings.Create(),
                SupportedMediaTypes =
                {
                    new MediaTypeHeaderValue("application/json") { CharSet = "utf-8" },
                    new MediaTypeHeaderValue("text/json") { CharSet = "utf-8" },
                },
            },
        };

        private readonly IAdapterIntegration _adapter;

        public BotMessageHandlerBase(IAdapterIntegration adapter)
        {
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        }

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
                    _adapter,
                    (context, ct) =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        IBot bot;

                        try
                        {
                            bot = (IBot)request.GetDependencyScope()?.GetService(typeof(IBot));
                        }
                        catch (Exception exception)
                        {
                            throw new Exception($"An exception occurred attempting to resolve an {nameof(IBot)} service via the dependency resolver. Please check the inner exception for more details.", exception);
                        }

                        if (bot == null)
                        {
                            throw new InvalidOperationException($"Did not find an {nameof(IBot)} service via the dependency resolver. Please make sure you have registered your bot with your dependency injection container.");
                        }

                        return bot.OnTurnAsync(context, ct);
                    },
                    cancellationToken).ConfigureAwait(false);

                if (invokeResponse == null)
                {
                    return request.CreateResponse(HttpStatusCode.OK);
                }

                var response = request.CreateResponse((HttpStatusCode)invokeResponse.Status);

                if (invokeResponse.Body != null)
                {
                    response.Content = new ObjectContent(
                        invokeResponse.Body.GetType(),
                        invokeResponse.Body,
                        BotMessageMediaTypeFormatters[0]);
                }

                return response;
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

        protected abstract Task<InvokeResponse> ProcessMessageRequestAsync(HttpRequestMessage request, IAdapterIntegration adapter, BotCallbackHandler botCallbackHandler, CancellationToken cancellationToken);
    }
}
