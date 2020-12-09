// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Runtime.Builders.Middleware
{
    /// <summary>
    /// Defines an implementation of <see cref="IMiddlewareBuilder"/> that returns an instance
    /// of <see cref="InspectionMiddleware"/>.
    /// </summary>
    [JsonObject]
    internal class InspectionMiddlewareBuilder : IMiddlewareBuilder
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.InspectionMiddleware";

        /// <summary>
        /// Builds an instance of type <see cref="InspectionMiddleware"/>.
        /// </summary>
        /// <param name="services">
        /// Provider containing all services registered with the application's service collection.
        /// </param>
        /// <param name="configuration">Application configuration.</param>
        /// <returns>An instance of type <see cref="InspectionMiddleware"/>.</returns>
        public IMiddleware Build(IServiceProvider services, IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var storage = services.GetService<IStorage>();

            return new InspectionMiddleware(new InspectionState(storage));
        }
    }
}
