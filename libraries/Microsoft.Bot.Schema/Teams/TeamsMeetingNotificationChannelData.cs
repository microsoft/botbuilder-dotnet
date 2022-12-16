﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Container for list of <see cref="OnBehalfOf"/> information for a meeting <see cref="TeamsMeetingNotificationChannelData"/>.
    /// </summary>
    public partial class TeamsMeetingNotificationChannelData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsMeetingNotificationChannelData"/> class.
        /// </summary>
        public TeamsMeetingNotificationChannelData()
        {
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the <see cref="OnBehalfOf"/> for user attribution.
        /// </summary>
        /// <value>The meeting notification's <see cref="OnBehalfOf"/>.</value>
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)>
        [JsonProperty(PropertyName = "OnBehalfOf")]
        public IList<TeamsMeetingNotificationOnBehalfOf> OnBehalfOf { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
