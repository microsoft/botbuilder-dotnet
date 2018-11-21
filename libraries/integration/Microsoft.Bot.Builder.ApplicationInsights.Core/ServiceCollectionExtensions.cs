// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Bot.Builder.ApplicationInsights.Core
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds and configures services for Application Insights to the <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> which specifies the contract for a collection of service descriptors.</param>
        /// <param name="instrumentationKey">The Application Insights instrumentation key to use.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddBotApplicationInsightsTelemetryClient(this IServiceCollection services, string instrumentationKey)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services.AddBotApplicationInsightsTelemetryClient(tc => tc.InstrumentationKey = instrumentationKey);
        }

        public static IServiceCollection AddBotApplicationInsightsTelemetryClient(this IServiceCollection services, Action<TelemetryConfiguration> configure = null)
        {
            var applicationInsightsTelemetryClient = CreateApplicationInsightsTelemetryClient(configure);

            services.AddSingleton<IBotTelemetryClient>(new BotTelemetryClient(applicationInsightsTelemetryClient));

            // Add a post configure call back to hook up our middleware
            services.PostConfigure<BotFrameworkOptions>(options =>
            {
                // Always add ourselves as the first piece of middleware to ensure we make our telemetry data available ASAP
                options.Middleware.Insert(0, new BotActivityTelemetryMiddleware(applicationInsightsTelemetryClient));
            });

            return services;

        }


        // REVISIT: QUESTIONABLE THAT THIS SHOULD BE HERE - let the app do this work itself if it wants to, remove direct ties to Microsoft.Bot.Configuration
        public static IServiceCollection AddBotApplicationInsightsTelemetryClient(this IServiceCollection services, BotConfiguration botConfiguration, string appInsightsServiceInstanceName = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (botConfiguration == null)
            {
                throw new ArgumentNullException(nameof(botConfiguration));
            }

            var appInsightsServices = botConfiguration.Services.OfType<AppInsightsService>();

            var instanceNameSpecified = appInsightsServiceInstanceName != null;

            if (instanceNameSpecified)
            {
                appInsightsServices = appInsightsServices.Where(ais => ais.Name == appInsightsServiceInstanceName);
            }

            var appInsightsService = appInsightsServices.FirstOrDefault();

            if(appInsightsService == null)
            {
                var message = instanceNameSpecified ?
                                    $"No Application Insights Service instance with the specified name \"{appInsightsServiceInstanceName}\" was found in the {nameof(BotConfiguration)}"
                                        : 
                                    $"No Application Insights Service instance was found in the {nameof(BotConfiguration)}.";

                throw new Exception();
            }

            return services.AddBotApplicationInsightsTelemetryClient(tc => tc.InstrumentationKey = appInsightsService.InstrumentationKey);
        }

        private static TelemetryClient CreateApplicationInsightsTelemetryClient(Action<TelemetryConfiguration> configure)
        {
            var telemetryConfiguration = new TelemetryConfiguration();

            var telemetryInitializers = telemetryConfiguration.TelemetryInitializers;
            telemetryInitializers.Add(new OperationCorrelationTelemetryInitializer());

            configure?.Invoke(telemetryConfiguration);

            return new TelemetryClient(telemetryConfiguration);
        }
    }
}
