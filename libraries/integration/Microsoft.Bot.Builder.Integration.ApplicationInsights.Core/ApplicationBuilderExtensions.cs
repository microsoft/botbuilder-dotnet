// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.AspNetCore.Builder;

namespace Microsoft.Bot.Builder.Integration.ApplicationInsights.Core
{
    /// <summary>
    /// ApplicationBuilder extension methods for use when registering Application Insights services at startup.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Constant key used for Application Insights Instrumentation Key.
        /// </summary>
        public const string AppInsightsInstrumentationKey = "ApplicationInsights:InstrumentationKey";

        /// <summary>
        /// Adds Telemetry ASP.Net Middleware.
        /// </summary>
        /// <param name="applicationBuilder">The <see cref="IApplicationBuilder"/>.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        // This class has been deprecated in favor of using TelemetryInitializerMiddleware in
        // Microsoft.Bot.Integration.ApplicationInsights.Core and Microsoft.Bot.Integration.ApplicationInsights.WebApi
        [Obsolete("This class is deprecated. Please add TelemetryInitializerMiddleware to your adapter's middleware pipeline instead.")]
        public static IApplicationBuilder UseBotApplicationInsights(this IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
            {
                throw new ArgumentNullException(nameof(applicationBuilder));
            }

            applicationBuilder.UseMiddleware<TelemetrySaveBodyASPMiddleware>();

            return applicationBuilder;
        }
    }
}
