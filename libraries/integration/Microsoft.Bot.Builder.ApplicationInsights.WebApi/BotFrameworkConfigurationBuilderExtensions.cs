using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Bot.Builder.ApplicationInsights.Core;

namespace Microsoft.Bot.Builder.Integration.AspNet.WebApi
{
    public static class BotFrameworkConfigurationBuilderExtensions
    {
        public static BotFrameworkConfigurationBuilder UseApplicationInsightsBotTelemetry(this BotFrameworkConfigurationBuilder builder, string instrumentationKey) =>
            builder.UseApplicationInsightsBotTelemetry(tc => tc.InstrumentationKey = instrumentationKey);

        public static BotFrameworkConfigurationBuilder UseApplicationInsightsBotTelemetry(this BotFrameworkConfigurationBuilder builder, Action<TelemetryConfiguration> configure)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.UseMiddleware(middlewareSet =>
            {
                middlewareSet.Insert(0, new BotActivityTelemetryMiddleware(CreateApplicationInsightsTelemetryClient(configure)));
            });
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
