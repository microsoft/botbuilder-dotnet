// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;

    /// <summary>
    /// Specifies attribution for notifications.
    /// </summary>
    public partial class TeamsMeetingNotificationOnBehalfOf
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsMeetingNotificationOnBehalfOf"/> class.
        /// </summary>
        public TeamsMeetingNotificationOnBehalfOf()
        {
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the identification of the item. Default is 0.
        /// </summary>
        /// <value>The item id.</value>
        [JsonProperty(PropertyName = "itemId")]
        public int ItemId { get; set; } = 0;

        /// <summary>
        /// Gets or sets the mention type. Default is "person".
        /// </summary>
        /// <value>The mention type.</value>
        [JsonProperty(PropertyName = "mentionType")]
        public string MentionType { get; set; } = "person";

        /// <summary>
        /// Gets or sets message resource identifier (MRI) of the person on whose behalf the message is sent.
        /// Message sender name would appear as "[user] through [bot name]".
        /// </summary>
        /// <value>The message resource identifier of the person.</value>
        [JsonProperty(PropertyName = "mri")]
        public string Mri { get; set; }

        /// <summary>
        /// Gets or sets name of the person. Used as fallback in case name resolution is unavailable.
        /// </summary>
        /// <value>The name of the person.</value>
        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
