// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseBotFramework(this IApplicationBuilder applicationBuilder)
        {
            var options = applicationBuilder.ApplicationServices.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;

            var botFrameworkAdapter = new BotFrameworkAdapter(options.ApplicationId, options.ApplicationPassword);

            foreach (var middleware in options.Middleware)
            {
                botFrameworkAdapter.Use(middleware);
            }

            var botActivitiesPath = new PathString(options.RouteBaseUrl);

            botActivitiesPath.Add("/messages");

            applicationBuilder.Map(
                botActivitiesPath, 
                botActivitiesAppBuilder => botActivitiesAppBuilder.Run(new BotActivitiesHandler(botFrameworkAdapter).HandleAsync));

            return applicationBuilder;

            
        }
    }
}
