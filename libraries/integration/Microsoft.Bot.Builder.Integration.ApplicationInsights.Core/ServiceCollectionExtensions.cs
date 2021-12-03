// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Bot.Builder.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.Builder.Integration.ApplicationInsights.Core
{
    /// <summary>
    /// Services collection extension methods for use when configuring Application Insights at startup.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds and configures services for Application Insights to the <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> which specifies the contract for a collection of service descriptors.</param>
        /// <param name="config">Represents a set of key/value application configuration properties.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddBotApplicationInsights(this IServiceCollection services, IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            string instrumentationKey = config.GetValue<string>("ApplicationInsights:instrumentationKey");

            CreateBotTelemetry(services);

            IBotTelemetryClient telemetryClient = null;
            if (!string.IsNullOrWhiteSpace(instrumentationKey))
            {
                services.AddApplicationInsightsTelemetry(instrumentationKey);
                telemetryClient = new BotTelemetryClient(new TelemetryClient());
            }
            else
            {
                telemetryClient = NullBotTelemetryClient.Instance;
            }

            services.AddSingleton(telemetryClient);

            return services;
        }

        /// <summary>
        /// Adds and configures services for Application Insights to the <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> which specifies the contract for a collection of service descriptors.</param>
        /// <param name="botTelemetryClient">Bot Telemetry Client that logs event information.</param>
        /// <param name="instrumentationKey">If Bot Telemetry Client is using Application Insights, provide the instrumentation key.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddBotApplicationInsights(this IServiceCollection services, IBotTelemetryClient botTelemetryClient, string instrumentationKey = null)
        {
            if (botTelemetryClient == null)
            {
                throw new ArgumentNullException(nameof(botTelemetryClient));
            }

            CreateBotTelemetry(services);

            // Start Application Insights
            if (instrumentationKey != null)
            {
                services.AddApplicationInsightsTelemetry(instrumentationKey);
            }

            // Register the BotTelemetryClient
            services.AddSingleton(botTelemetryClient);

            return services;
        }

        /// <summary>
        /// Adds and configures services for Application Insights to the <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> which specifies the contract for a collection of service descriptors.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddBotApplicationInsights(this IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry();
            services.AddSingleton<IBotTelemetryClient, BotTelemetryClient>();
            CreateBotTelemetry(services);
            return services;
        }

        private static void CreateBotTelemetry(IServiceCollection services)
        {
            // Enables Bot Telemetry to save user/session id's as the bot user id and session
            services.AddTransient<TelemetrySaveBodyASPMiddleware>();
            services.AddSingleton<ITelemetryInitializer, OperationCorrelationTelemetryInitializer>();
            services.AddSingleton<ITelemetryInitializer, TelemetryBotIdInitializer>();
        }
    }
}
