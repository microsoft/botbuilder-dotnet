// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Describes a logging client for bot telemetry.
    /// </summary>
    public interface IBotPageViewTelemetryClient
    {
        /// <summary>
        /// Logs an Application Insights page view.
        /// </summary>
        /// <param name="dialogName">The name of the dialog to log the entry / start for.</param>
        /// <param name="properties">Named string values you can use to search and classify events.</param>
        /// <param name="metrics">Measurements associated with this event.</param>
        void TrackPageView(string dialogName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null);
    }
}
