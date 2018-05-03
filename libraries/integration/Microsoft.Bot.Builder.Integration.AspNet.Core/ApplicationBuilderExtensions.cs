// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    /// <summary>
    /// Extension methods for <see cref="IApplicationBuilder"/> to add a Bot to the ASP.NET Core request execution pipeline.
    /// </summary>
    /// <seealso cref="BotFrameworkPaths"/>
    /// <seealso cref="BotFrameworkAdapter"/>
    /// <seealso cref="ServiceCollectionExtensions"/>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Maps various endpoint handlers for the <see cref="ServiceCollectionExtensions.AddBot{TBot}(IServiceCollection, Action{BotFrameworkOptions})">registered bot</see> into the request execution pipeline.
        /// </summary>
        /// <param name="appicationBuilder">The <see cref="IApplicationBuilder"/>.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <remarks>
        ///     This maps the bot using a default set of endpoints. To control the exact paths you would
        ///     prefer the bot's endpoints to be exposed at, use the <see cref="UseBotFramwork(IApplicationBuilder, Action{BotFrameworkPaths})"/> 
        ///     overload instead.
        /// </remarks>
        public static IApplicationBuilder UseBotFramework(this IApplicationBuilder applicationBuilder) =>
            applicationBuilder.UseBotFramework(paths => {});

        /// <summary>
        /// Maps various endpoint handlers for the <see cref="ServiceCollectionExtensions.AddBot{TBot}(IServiceCollection, Action{BotFrameworkOptions})">registered bot</see> into the request execution pipeline.
        /// </summary>
        /// <param name="appicationBuilder">The <see cref="IApplicationBuilder"/>.</param>
        /// <param name="configurePaths">A callback to configure the paths that determine where the endpoints of the bot will be exposed.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <seealso cref="ServiceCollectionExtensions.AddBot{TBot}(IServiceCollection, Action{BotFrameworkOptions})"/>
        /// <seealso cref="BotFrameworkPaths"/>
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

            var delegatingHandler = applicationBuilder.ApplicationServices.GetService<DelegatingHandler>();

            // Even if Delegating Handler is null we have handling down the pipeline to take care of it.
            BotFrameworkAdapter botFrameworkAdapter = new BotFrameworkAdapter(options.CredentialProvider, options.ConnectorClientRetryPolicy, delegatingHandler);

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
