// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Handlers
{
    /// <summary>
    /// Abstract base class for a bot message handler.
    /// </summary>
    public abstract class BotMessageHandlerBase
    {
        /// <summary>
        /// A <see cref="JsonSerializer"/> for use when serializing bot messages.
        /// </summary>
        public static readonly JsonSerializer BotMessageSerializer = JsonSerializer.Create(MessageSerializerSettings.Create());

        /// <summary>
        /// Initializes a new instance of the <see cref="BotMessageHandlerBase"/> class.
        /// </summary>
        public BotMessageHandlerBase()
        {
        }

        /// <summary>
        /// Handles common behavior for handling requests, including checking valid request method and content type.
        /// Processes the request using the registered adapter and bot and writes the result to the response on the <see cref="HttpContext"/>.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext"/>.</param>
        /// <returns>A Task that represents the work to be executed.</returns>
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

            var requestServices = httpContext.RequestServices;
            var adapter = requestServices.GetRequiredService<IAdapterIntegration>();
            var bot = requestServices.GetRequiredService<IBot>();

            try
            {
                var invokeResponse = await ProcessMessageRequestAsync(
                    request,
                    adapter,
                    bot.OnTurnAsync,
                    default(CancellationToken)).ConfigureAwait(false);

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
                            using (var writer = new StreamWriter(memoryStream, Encoding.UTF8, 1024, true))
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
            catch (UnauthorizedAccessException)
            {
                response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        /// <summary>
        /// Abstract method to process the incoming request using the registered adapter and bot and
        /// to return an <see cref="InvokeResponse"/>.
        /// </summary>
        /// <param name="request">A <see cref="HttpRequest"/>.</param>
        /// <param name="adapter">An instance of <see cref="IAdapterIntegration"/>.</param>
        /// <param name="botCallbackHandler">An instance of <see cref="BotCallbackHandler"/>.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
        /// <returns>An <see cref="InvokeResponse"/> returned from the adapter.</returns>
        protected abstract Task<InvokeResponse> ProcessMessageRequestAsync(HttpRequest request, IAdapterIntegration adapter, BotCallbackHandler botCallbackHandler, CancellationToken cancellationToken);
    }
}
