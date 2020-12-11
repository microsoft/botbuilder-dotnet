// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Properties;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Bot.Builder.ApplicationInsights;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
using Microsoft.Bot.Builder.Runtime.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Runtime.Providers.Telemetry
{
    /// <summary>
    /// Defines an implementation of <see cref="ITelemetryProvider"/> that configures the application
    /// to utilize Application Insights for telemetry, as well as registers
    /// <see cref="OperationCorrelationTelemetryInitializer"/>, <see cref="TelemetryBotIdInitializer"/> and
    /// <see cref="BotTelemetryClient"/> with the application's service collection.
    /// </summary>
    [JsonObject]
    internal class ApplicationInsightsTelemetryProvider : ITelemetryProvider
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.ApplicationInsightsTelemetryProvider";

        /// <summary>
        /// Gets or sets the Application Insights instrumentation key to use for telemetry.
        /// </summary>
        /// <value>
        /// The Application Insights instrumentation key to use for telemetry.
        /// </value>
        [JsonProperty("instrumentationKey")]
        public StringExpression InstrumentationKey { get; set; }

        /// <summary>
        /// Register services with the application's service collection.
        /// </summary>
        /// <param name="services">The application's collection of registered services.</param>
        /// <param name="configuration">Application configuration.</param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            services.AddApplicationInsightsTelemetry(this.InstrumentationKey?.GetConfigurationValue(configuration));
            services.AddSingleton<ITelemetryInitializer, OperationCorrelationTelemetryInitializer>();
            services.AddSingleton<ITelemetryInitializer, TelemetryBotIdInitializer>();
            services.AddSingleton<IBotTelemetryClient, BotTelemetryClient>();
        }
    }
}
