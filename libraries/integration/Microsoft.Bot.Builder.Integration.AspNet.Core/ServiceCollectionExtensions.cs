// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    /// <summary>
    /// Extension class for bot integration with ASP.NET Core 2.0 projects.
    /// </summary>
    /// <seealso cref="ApplicationBuilderExtensions"/>
    /// <seealso cref="BotAdapter"/>
    public static class ServiceCollectionExtensions
    {
        private static readonly JsonSerializer ActivitySerializer = JsonSerializer.Create();

        /// <summary>
        /// Adds a bot service to the ASP.NET container.
        /// </summary>
        /// <typeparam name="TBot">The type of the bot to add.</typeparam>
        /// <param name="services">The services collection for the ASP.NET application.</param>
        /// <param name="setupAction">The delegate to run after an instance of the bot is added to the collection.</param>
        /// <returns>The updated services collection.</returns>
        /// <remarks>This method adds a default instance of <typeparamref name="TBot"/> as a transient service.</remarks>
        public static IServiceCollection AddBot<TBot>(this IServiceCollection services, Action<BotFrameworkOptions> setupAction = null) where TBot : class, IBot
        {
            services.AddTransient<IBot, TBot>();

            services.Configure(setupAction);

            return services;
        }
    }
}
