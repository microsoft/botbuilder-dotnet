// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Properties;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
using Microsoft.Bot.Builder.Runtime.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Runtime.Builders.Middleware
{
    /// <summary>
    /// Defines an implementation of <see cref="IMiddlewareBuilder"/> that returns an instance
    /// of <see cref="TelemetryInitializerMiddleware"/>.
    /// </summary>
    [JsonObject]
    internal class TelemetryMiddlewareBuilder : IMiddlewareBuilder
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.TelemetryMiddleware";

        /// <summary>
        /// Gets or sets whether to enable logging of activity events. Defaults to true.
        /// </summary>
        /// <value>
        /// Indicates whether to enable logging of activity events. Defaults to true.
        /// </value>
        [JsonProperty("logActivities")]
        public BoolExpression LogActivities { get; set; }

        /// <summary>
        /// Gets or sets whether include personally identifiable information (PII) in log events. Defaults to false.
        /// </summary>
        /// <value>
        /// Indicates whether include personally identifiable information (PII) in log events. Defaults to false.
        /// </value>
        [JsonProperty("logPersonalInformation")]
        public BoolExpression LogPersonalInformation { get; set; }

        /// <summary>
        /// Builds an instance of type <see cref="TelemetryInitializerMiddleware"/>.
        /// </summary>
        /// <param name="services">
        /// Provider containing all services registered with the application's service collection.
        /// </param>
        /// <param name="configuration">Application configuration.</param>
        /// <returns>An instance of type <see cref="TelemetryInitializerMiddleware"/>.</returns>
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

            var botTelemetryClient = services.GetService<IBotTelemetryClient>();
            var httpContextAccessor = services.GetService<IHttpContextAccessor>();

            return new TelemetryInitializerMiddleware(
                httpContextAccessor: httpContextAccessor,
                telemetryLoggerMiddleware: new TelemetryLoggerMiddleware(
                    telemetryClient: botTelemetryClient,
                    logPersonalInformation: this.LogPersonalInformation?.GetConfigurationValue(configuration) ?? false),
                logActivityTelemetry: this.LogActivities?.GetConfigurationValue(configuration) ?? true);
        }
    }
}
