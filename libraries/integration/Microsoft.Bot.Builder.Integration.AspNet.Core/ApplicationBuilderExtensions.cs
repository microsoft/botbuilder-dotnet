// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    public static class ApplicationBuilderExtensions
    {
        private static readonly JsonSerializer ActivitySerializer = JsonSerializer.Create();

        public static IApplicationBuilder UseBotFramework(this IApplicationBuilder applicationBuilder)
        {
            var options = applicationBuilder.ApplicationServices.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;

            var botFrameworkAdapter = new BotFrameworkAdapter(options.CredentialProvider);

            foreach (var middleware in options.Middleware)
            {
                botFrameworkAdapter.Use(middleware);
            }

            var botActivitiesPath = new PathString(options.RouteBaseUrl);

            botActivitiesPath.Add("/messages");

            applicationBuilder.Map(
                botActivitiesPath, 
                botActivitiesAppBuilder => botActivitiesAppBuilder.Run(BotRequestHandler));

            return applicationBuilder;

            async Task BotRequestHandler(HttpContext httpContext)
            {
                var request = httpContext.Request;
                var response = httpContext.Response;

                if(request.Method != HttpMethods.Post)
                {
                    response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;

                    return;
                }

                if(request.ContentType != "application/json")
                {
                    response.StatusCode = (int)HttpStatusCode.NotAcceptable;

                    return;
                }

                if(request.ContentLength == 0)
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;

                    return;
                }

                var activity = default(Activity);

                using (var bodyReader = new JsonTextReader(new StreamReader(request.Body, Encoding.UTF8)))
                {
                    activity = ActivitySerializer.Deserialize<Activity>(bodyReader);
                }

                try
                {
                    await botFrameworkAdapter.ProcessActivity(
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
}
