// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.Runtime.Builders
{
    /// <summary>
    /// Defines an interface used to build an instance of the specified type using supplied application
    /// configuration and registered services.
    /// </summary>
    /// <typeparam name="T">The type of the object to be returned by the builder.</typeparam>
    internal interface IBuilder<out T>
    {
        /// <summary>
        /// Builds an instance of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="services">
        /// Provider containing all services registered with the application's service collection.
        /// </param>
        /// <param name="configuration">Application configuration.</param>
        /// <returns>An instance of type <typeparamref name="T"/>.</returns>
        T Build(IServiceProvider services, IConfiguration configuration);
    }
}
