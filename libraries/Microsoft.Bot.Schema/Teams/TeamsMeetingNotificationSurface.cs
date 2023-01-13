// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;

    /// <summary>
    /// Specifies if a notification is to be sent for the mentions.
    /// </summary>
    public partial class TeamsMeetingNotificationSurface
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsMeetingNotificationSurface"/> class.
        /// </summary>
        public TeamsMeetingNotificationSurface()
        {
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the value indicating where the signal will be rendered in the meeting UX.
        /// Note: only one instance of surface type is allowed per request.
        /// </summary>
        /// <value>
        /// The value indicating where the signal will be rendered in the meeting UX.
        /// </value>
        [JsonProperty(PropertyName = "surface")]
        public string Surface { get; set; } = "meetingStage";

        /// <summary>
        /// Gets or sets the content type of this <see cref="TeamsMeetingNotificationSurface"/>.
        /// </summary>
        /// <value>
        /// The content type of this <see cref="TeamsMeetingNotificationSurface"/>.
        /// </value>
        [JsonProperty(PropertyName = "contentType")]
        public string ContentType { get; set; } = "task";

        /// <summary>
        /// Gets or sets the content for this <see cref="TeamsMeetingNotificationSurface"/>.
        /// </summary>
        /// <value>
        /// The content type of this <see cref="TeamsMeetingNotificationSurface"/>.
        /// </value>
        [JsonProperty(PropertyName = "content")]
        public TaskModuleContinueResponse Content { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
