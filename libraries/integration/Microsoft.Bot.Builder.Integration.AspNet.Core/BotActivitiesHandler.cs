// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    internal class BotActivitiesHandler
    {
        private static readonly JsonSerializer ActivitySerializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Newtonsoft.Json.Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        });

        private BotFrameworkAdapter _botFrameworkAdapter;

        public BotActivitiesHandler(BotFrameworkAdapter botFrameworkAdapter)
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

            var activity = default(Activity);

            using (var bodyReader = new JsonTextReader(new StreamReader(request.Body, Encoding.UTF8)))
            {
                activity = ActivitySerializer.Deserialize<Activity>(bodyReader);
            }

            try
            {
                await _botFrameworkAdapter.ProcessActivity(
                    request.Headers["Authorization"],
                    activity,
                    botContext =>
                    {
                        var bot = httpContext.RequestServices.GetRequiredService<IBot>();

                        return bot.OnReceiveActivity(botContext);
                    });

                response.StatusCode = (int)HttpStatusCode.OK;
            }
            catch (UnauthorizedAccessException)
            {
                response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}