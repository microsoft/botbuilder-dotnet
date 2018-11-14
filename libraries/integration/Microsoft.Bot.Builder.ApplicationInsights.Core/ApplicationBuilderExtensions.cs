// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Microsoft.Bot.Builder.ApplicationInsights.Core
{
    public static class ApplicationBuilderExtensions
    {
        public const string AppInsightsInstrumentationKey = "ApplicationInsights:InstrumentationKey";

        /// <summary>
        /// Adds Telemetry ASP.Net Middleware.
        /// </summary>
        /// <param name="applicationBuilder">The <see cref="IApplicationBuilder"/>.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder UseBotApplicationInsights(this IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
            {
                throw new ArgumentNullException(nameof(applicationBuilder));
            }

            var applicationServices = applicationBuilder.ApplicationServices;

            var configuration = applicationServices.GetService<IConfiguration>();

            if (configuration != null)
            {
                var instrumentationKey = configuration.GetSection(AppInsightsInstrumentationKey)?.Value;

                if (string.IsNullOrEmpty(instrumentationKey))
                {
                    throw new InvalidOperationException("The appsettings.json file is missng the Application Insights instrumentation key.");
                }
            }

            applicationBuilder.UseMiddleware<TelemetrySaveBodyASPMiddleware>();

            return applicationBuilder;
        }

    }
}
