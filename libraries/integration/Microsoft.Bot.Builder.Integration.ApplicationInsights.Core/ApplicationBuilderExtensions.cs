// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.AspNetCore.Builder;

namespace Microsoft.Bot.Builder.Integration.ApplicationInsights.Core
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

            applicationBuilder.UseMiddleware<TelemetrySaveBodyASPMiddleware>();

            return applicationBuilder;
        }
    }
}
