// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseBotFramework(this IApplicationBuilder applicationBuilder) =>
            applicationBuilder.UseBotFramework(paths => {});

        public static IApplicationBuilder UseBotFramework(this IApplicationBuilder applicationBuilder, Action<BotFrameworkPaths> configurePaths)
        {
            if (applicationBuilder == null)
            {
                throw new ArgumentNullException(nameof(applicationBuilder));
            }

            if (configurePaths == null)
            {
                throw new ArgumentNullException(nameof(configurePaths));
            }

            var options = applicationBuilder.ApplicationServices.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;

            var botFrameworkAdapter = new BotFrameworkAdapter(options.CredentialProvider);

            foreach (var middleware in options.Middleware)
            {
                botFrameworkAdapter.Use(middleware);
            }

            var paths = new BotFrameworkPaths();

            configurePaths(paths);

            if (options.EnableProactiveMessages)
            {
                applicationBuilder.Map(
                    paths.BasePath + paths.ProactivePath,
                    botProactiveAppBuilder => botProactiveAppBuilder.Run(httpContext => { httpContext.Response.StatusCode = (int)HttpStatusCode.OK; return Task.CompletedTask; }));
            }

            applicationBuilder.Map(
                paths.BasePath + paths.ActivitiesPath, 
                botActivitiesAppBuilder => botActivitiesAppBuilder.Run(new BotActivitiesHandler(botFrameworkAdapter).HandleAsync));

            return applicationBuilder;

            
        }
    }
}
