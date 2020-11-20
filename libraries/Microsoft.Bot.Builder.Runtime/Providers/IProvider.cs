// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.Builder.Runtime.Providers
{
    /// <summary>
    /// Defines an interface used to register one or more services with the application's
    /// <see cref="IServiceCollection"/> utilizing provided application configuration via <see cref="IConfiguration"/>.
    /// </summary>
    public interface IProvider
    {
        /// <summary>
        /// Register services with the application's service collection.
        /// </summary>
        /// <param name="services">The application's collection of registered services.</param>
        /// <param name="configuration">Application configuration.</param>
        void ConfigureServices(IServiceCollection services, IConfiguration configuration);
    }
}
