// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.ApplicationInsights.Core
{
    public sealed class BotActivityTelemetryMiddleware : IMiddleware
    {
        private static readonly AsyncLocal<Activity> ActivityAsyncLocal = new AsyncLocal<Activity>();

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
                Name = "Bot Turn",
            };

            var telemetryContext = requestTelemetry.Context;

            telemetryContext.User.Id = activity.From.Id;
            telemetryContext.Session.Id = activity.Conversation.Id;

            requestTelemetry.Properties.Add("BotFramework.Activity.ChannelId", activity.ChannelId);
            requestTelemetry.Properties.Add("BotFramework.Activity.Type", activity.Type);

            // TODO: anything else we want to enrich with?

            requestTelemetry.Start();

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
                requestTelemetry.Stop();

                requestTelemetry.Metrics.Add("BotFramework.Responded", turnContext.Responded ? 1 : 0);

                _telemetryClient.Track(requestTelemetry);
            }
        }
    }
}
