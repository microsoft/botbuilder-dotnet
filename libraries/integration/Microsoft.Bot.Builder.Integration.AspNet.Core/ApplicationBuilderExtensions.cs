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
    /// <summary>
    /// Extension class for bot integration with ASP.NET Core 2.0 projects.
    /// </summary>
    /// <seealso cref="BotFrameworkPaths"/>
    /// <seealso cref="BotFrameworkAdapter"/>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Initializes and adds a bot adapter to the HTTP request pipeline, using default endpoint paths for the bot.
        /// </summary>
        /// <param name="applicationBuilder">The application builder for the ASP.NET application.</param>
        /// <returns>The updated application builder.</returns>
        /// <remarks>This method adds any middleware from the <see cref="BotFrameworkOptions"/> provided in the
        /// <see cref="ServiceCollectionExtensions.AddBot{TBot}(IServiceCollection, Action{BotFrameworkOptions})"/>
        /// method to the adapter.</remarks>
        public static IApplicationBuilder UseBotFramework(this IApplicationBuilder applicationBuilder) =>
            applicationBuilder.UseBotFramework(paths => {});

        /// <summary>
        /// Initializes and adds a bot adapter to the HTTP request pipeline, using custom endpoint paths for the bot.
        /// </summary>
        /// <param name="applicationBuilder">The application builder for the ASP.NET application.</param>
        /// <param name="configurePaths">Allows you to modify the endpoints for the bot.</param>
        /// <returns>The updated application builder.</returns>
        /// <remarks>This method adds any middleware from the <see cref="BotFrameworkOptions"/> provided in the
        /// <see cref="ServiceCollectionExtensions.AddBot{TBot}(IServiceCollection, Action{BotFrameworkOptions})"/>
        /// method to the adapter.</remarks>
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
