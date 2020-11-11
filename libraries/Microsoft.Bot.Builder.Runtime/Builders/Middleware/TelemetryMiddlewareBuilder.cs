// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Properties;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
using Microsoft.Bot.Builder.Runtime.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using IMiddleware = Microsoft.Bot.Builder.IMiddleware;

namespace Microsoft.Bot.Builder.Runtime.Builders.Middleware
{
    [JsonObject]
    public class TelemetryMiddlewareBuilder : IMiddlewareBuilder
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.TelemetryMiddleware";

        [JsonProperty("logActivities")]
        public BoolExpression LogActivities { get; set; }

        [JsonProperty("logPersonalInformation")]
        public BoolExpression LogPersonalInformation { get; set; }

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
