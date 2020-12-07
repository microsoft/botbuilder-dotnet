// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Runtime.Extensions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Runtime.Builders.Middleware
{
    /// <summary>
    /// Defines an implementation of <see cref="IMiddlewareBuilder"/> that returns an instance
    /// of <see cref="ShowTypingMiddleware"/>.
    /// </summary>
    [JsonObject]
    public class ShowTypingMiddlewareBuilder : IMiddlewareBuilder
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.ShowTypingMiddleware";

        private const int DefaultDelay = 500;
        private const int DefaultPeriod = 2000;

        /// <summary>
        /// Gets or sets the duration in milliseconds to delay before sending the first typing indicator.
        /// Defaults to 500.
        /// </summary>
        /// <value>
        /// The duration in milliseconds to delay before sending the first typing indicator.
        /// Defaults to 500.
        /// </value>
        [JsonProperty("delay")]
        public IntExpression Delay { get; set; }

        /// <summary>
        /// Gets or sets the interval in milliseconds at which additional typing indicators will be sent.
        /// Defaults to 2000.
        /// </summary>
        /// <value>
        /// The interval in milliseconds at which additional typing indicators will be sent.
        /// Defaults to 2000.
        /// </value>
        [JsonProperty("period")]
        public IntExpression Period { get; set; }

        /// <summary>
        /// Builds an instance of type <see cref="ShowTypingMiddleware"/>.
        /// </summary>
        /// <param name="services">
        /// Provider containing all services registered with the application's service collection.
        /// </param>
        /// <param name="configuration">Application configuration.</param>
        /// <returns>An instance of type <see cref="ShowTypingMiddleware"/>.</returns>
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

            return new ShowTypingMiddleware(
                delay: this.Delay?.GetConfigurationValue(configuration) ?? DefaultDelay,
                period: this.Period?.GetConfigurationValue(configuration) ?? DefaultPeriod);
        }
    }
}
