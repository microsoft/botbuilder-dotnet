// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Microsoft.Bot.Builder.ApplicationInsights.Core
{
    public sealed class BotActivityTelemetryMiddleware : IMiddleware
    {
        private readonly TelemetryClient _telemetryClient;

        public BotActivityTelemetryMiddleware(TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient ?? throw new System.ArgumentNullException(nameof(telemetryClient));

            // TODO: consider initializing some global properties such as SDK version?
        }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            var activity = turnContext.Activity;

            var requestTelemetry = new RequestTelemetry
            {
                Id = activity.Id,
                Name = "Bot Turn"
            };

            requestTelemetry.Start();

            var telemetryContext = requestTelemetry.Context;

            telemetryContext.User.Id = activity.From.Id;
            telemetryContext.Session.Id = activity.Conversation.Id;

            var telemetryProperties = requestTelemetry.Properties;
            telemetryProperties.Add("BotFramework.Activity.ChannelId", activity.ChannelId);
            telemetryProperties.Add("BotFramework.Activity.Type", activity.Type);

            // TODO: anything else we want to enrich with?


            try
            {
                await next(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                TrackTelemetry(false);

                throw;
            }

            TrackTelemetry(true);

            void TrackTelemetry(bool success)
            {
                requestTelemetry.Success = success;
                requestTelemetry.Metrics.Add("BotFramework.Responded", turnContext.Responded ? 1 : 0);

                requestTelemetry.Stop();

                _telemetryClient.Track(requestTelemetry);
            }
        }
    }
}
