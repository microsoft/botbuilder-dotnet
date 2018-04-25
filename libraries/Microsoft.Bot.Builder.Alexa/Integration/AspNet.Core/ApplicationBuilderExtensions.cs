// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Bot.Builder.Alexa.Integration.AspNet.Core
{
    /// <summary>
    /// Extension class for bot integration with ASP.NET Core 2.0 projects.
    /// </summary>
    /// <seealso cref="AlexaBotPaths"/>
    /// <seealso cref="AlexaAdapter"/>
    /// <seealso cref="ServiceCollectionExtensions"/>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Initializes and adds a bot adapter to the HTTP request pipeline, using default endpoint paths for the bot.
        /// </summary>
        /// <param name="applicationBuilder">The application builder for the ASP.NET application.</param>
        /// <returns>The updated application builder.</returns>
        /// <remarks>This method adds any middleware from the <see cref="AlexaBotOptions"/> provided in the
        /// <see cref="ServiceCollectionExtensions.AddAlexaBot{TBot}(IServiceCollection, Action{AlexaBotOptions})"/>
        /// method to the adapter.</remarks>
        public static IApplicationBuilder UseAlexa(this IApplicationBuilder applicationBuilder) =>
            applicationBuilder.UseAlexa(paths => {});

        /// <summary>
        /// Initializes and adds a bot adapter to the HTTP request pipeline, using custom endpoint paths for the bot.
        /// </summary>
        /// <param name="applicationBuilder">The application builder for the ASP.NET application.</param>
        /// <param name="configurePaths">Allows you to modify the endpoints for the bot.</param>
        /// <returns>The updated application builder.</returns>
        /// <remarks>This method adds any middleware from the <see cref="AlexaBotOptions"/> provided in the
        /// <see cref="ServiceCollectionExtensions.AddBot{TBot}(IServiceCollection, Action{AlexaBotOptions})"/>
        /// method to the adapter.</remarks>
        public static IApplicationBuilder UseAlexa(this IApplicationBuilder applicationBuilder, Action<AlexaBotPaths> configurePaths)
        {
            if (applicationBuilder == null)
            {
                throw new ArgumentNullException(nameof(applicationBuilder));
            }

            if (configurePaths == null)
            {
                throw new ArgumentNullException(nameof(configurePaths));
            }

            var options = applicationBuilder.ApplicationServices.GetRequiredService<IOptions<AlexaBotOptions>>().Value;

            var alexaAdapter = new AlexaAdapter();

            foreach (var middleware in options.Middleware)
            {
                alexaAdapter.Use(middleware);
            }

            var paths = options.Paths;

            configurePaths(paths);

            applicationBuilder.Map(
                $"{paths.BasePath}/{paths.SkillRequestsPath}", 
                botActivitiesAppBuilder => botActivitiesAppBuilder.Run(new AlexaRequestHandler(alexaAdapter, options.ValidateIncomingAlexaRequests).HandleAsync));

            return applicationBuilder;
        }
    }
}
