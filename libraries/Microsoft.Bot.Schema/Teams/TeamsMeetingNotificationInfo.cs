// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.Teams
{
    /// <summary>
    /// Specifies the container for what is required to send a meeting notification to recipients.
    /// </summary>
    public partial class TeamsMeetingNotificationInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsMeetingNotificationInfo"/> class.
        /// </summary>
        public TeamsMeetingNotificationInfo()
        {
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the collection of recipients of the notification.
        /// </summary>
        /// <value>
        /// The collection of recipients of the notification.
        /// </value>
        [JsonProperty(PropertyName = "recipients")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public IList<string> Recipients { get; set; }

        /// <summary>
        /// Gets or sets the collection of surfaces on which to show the notification.
        /// </summary>
        /// <value>
        /// The collection of surfaces on which to show the notification.
        /// </value>
        [JsonProperty(PropertyName = "surfaces")]
        public IList<TeamsMeetingNotificationSurface> Surfaces { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
