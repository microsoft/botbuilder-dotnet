// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.ApplicationInsights
{
    public class TelemetryWaterfallDialog : WaterfallDialog
    {
        private readonly IBotTelemetryClient _telemetryClient;

        public TelemetryWaterfallDialog(string dialogId, IBotTelemetryClient telemetryClient, IEnumerable<WaterfallStep> steps = null)
            : base(dialogId, steps)
        {
            if (telemetryClient == null)
            {
                throw new ArgumentNullException(nameof(telemetryClient));
            }

            _telemetryClient = telemetryClient;
        }

        protected override async Task<DialogTurnResult> OnStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _telemetryClient.TrackWaterfallStep(stepContext, null);
            return await base.OnStepAsync(stepContext, cancellationToken);
        }

    }
}
