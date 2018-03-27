// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

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

            var botFrameworkAdapter = new BotFrameworkAdapter(options.CredentialProvider, options.ConnectorClientRetryPolicy);

            foreach (var middleware in options.Middleware)
            {
                botFrameworkAdapter.Use(middleware);
            }

            var paths = new BotFrameworkPaths();

            configurePaths(paths);

            if (options.EnableProactiveMessages)
            {
                applicationBuilder.Map(
                    paths.BasePath + paths.ProactiveMessagesPath,
                    botProactiveAppBuilder => botProactiveAppBuilder.Run(new BotProactiveMessageHandler(botFrameworkAdapter).HandleAsync));
            }

            applicationBuilder.Map(
                paths.BasePath + paths.MessagesPath, 
                botActivitiesAppBuilder => botActivitiesAppBuilder.Run(new BotMessageHandler(botFrameworkAdapter).HandleAsync));

            return applicationBuilder;

            
        }
    }
}
