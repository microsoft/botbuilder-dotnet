// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;

    /// <summary>
    /// Specifies meeting stage surface.
    /// </summary>
    public class MeetingTabIconSurface : Surface
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingTabIconSurface"/> class.
        /// </summary>
        public MeetingTabIconSurface()
            : base(SurfaceType.MeetingTabIcon)
        {
        }

        /// <summary>
        /// Gets or sets the tab entity Id of this <see cref="MeetingTabIconSurface"/>.
        /// </summary>
        /// <value>
        /// The tab entity Id of this <see cref="MeetingTabIconSurface"/>.
        /// </value>
        [JsonProperty("tabEntityId")]
        public string TabEntityId { get; set; }
    }
}
