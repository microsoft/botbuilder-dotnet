// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.ApplicationInsights.Core
{
    public sealed class ActivityTelemetryMiddleware : IMiddleware
    {
        private static readonly AsyncLocal<Activity> ActivityAsyncLocal = new AsyncLocal<Activity>();
        private readonly TelemetryClient _telemetryClient;

        public ActivityTelemetryMiddleware(TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient ?? throw new System.ArgumentNullException(nameof(telemetryClient));
        }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            SetActivityForCurrentTurn(turnContext.Activity);

            try
            {
                var requestTelemetry = new RequestTelemetry
                {
                    Id = turnContext.Activity.Id,
                    Name = "Bot Turn"
                };

                requestTelemetry.Start();

                try
                {
                    await next(cancellationToken).ConfigureAwait(false);

                    requestTelemetry.Success = true;
                }
                catch
                {
                    requestTelemetry.Success = false;

                    throw;
                }

                requestTelemetry.Stop();

                _telemetryClient.Track(requestTelemetry);
            }
            finally
            {
                SetActivityForCurrentTurn(null);
            }
        }

        internal static Activity GetActivityForCurrentTurn() =>
            ActivityAsyncLocal.Value;

        internal static void SetActivityForCurrentTurn(Activity activity) =>
            ActivityAsyncLocal.Value = activity;
    }
}
