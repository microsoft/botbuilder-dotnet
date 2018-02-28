// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Samples.EchoBot_AspNet461.Integration
{
    internal sealed class BotActivitiesHandler : HttpMessageHandler
    {
        private static readonly MediaTypeFormatter[] BotActivityMediaTypeFormatters = new [] {
            new JsonMediaTypeFormatter
            {
                SerializerSettings =
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Formatting = Newtonsoft.Json.Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore,
                }
            }
        };

        private readonly BotFrameworkAdapter _botFrameworkAdapter;

        public BotActivitiesHandler(BotFrameworkAdapter botFrameworkAdapter)
        {
            _botFrameworkAdapter = botFrameworkAdapter;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var activity = await request.Content.ReadAsAsync<Activity>(BotActivitiesHandler.BotActivityMediaTypeFormatters, cancellationToken);
            var dependencyScope = request.GetDependencyScope();

            try
            {
                await _botFrameworkAdapter.ProcessActivity(
                    request.Headers.Authorization?.Parameter,
                    activity,
                    botContext =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        IBot bot;

                        try
                        {
                            bot = (IBot)dependencyScope.GetService(typeof(IBot));
                        }
                        catch
                        {
                            bot = null;
                        }

                        if(bot == null)
                        {
                            throw new InvalidOperationException($"Did not find an {typeof(IBot).Name} service via the dependency resolver. Please make sure you have registered your bot with your dependency injection container.");
                        }

                        return bot.OnReceiveActivity(botContext);
                    });

                return request.CreateResponse(HttpStatusCode.OK);
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
    }
}