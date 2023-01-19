// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Specify Teams Bot meeting notification channel data.
    /// </summary>
    public partial class BotMeetingNotificationChannelData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BotMeetingNotificationChannelData"/> class.
        /// </summary>
        public BotMeetingNotificationChannelData()
        {
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the list of <see cref="OnBehalfOf"/> for user attribution.
        /// </summary>
        /// <value>The Teams Bot meeting notification's <see cref="OnBehalfOf"/>.</value>
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)>
        [JsonProperty(PropertyName = "OnBehalfOf")]
        public IList<OnBehalfOf> OnBehalfOf { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
