// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.Teams
{
    /// <summary>
    /// Specifies the targeted meeting notification value, including recipients and surfaces.
    /// </summary>
    public class TargetedMeetingNotificationValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TargetedMeetingNotificationValue"/> class.
        /// </summary>
        public TargetedMeetingNotificationValue()
        {
        }

        /// <summary>
        /// Gets or sets the collection of recipients of the targeted meeting notification.
        /// </summary>
        /// <value>
        /// The collection of recipients of the targeted meeting notification.
        /// </value>
        [JsonProperty(PropertyName = "recipients")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public IList<string> Recipients { get; set; }

        /// <summary>
        /// Gets or sets the collection of surfaces on which to show the notification.
        /// If a bot wants its content to be rendered in different surfaces areas, it can specific a list of UX areas. 
        /// But please note that only one instance of surface type is allowed per request. 
        /// </summary>
        /// <value>
        /// The collection of surfaces on which to show the notification.
        /// </value>
        [JsonProperty(PropertyName = "surfaces")]
        public IList<Surface> Surfaces { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }
}
