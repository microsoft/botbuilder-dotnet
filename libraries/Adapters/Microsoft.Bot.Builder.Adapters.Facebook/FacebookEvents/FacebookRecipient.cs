// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents
{
    /// <summary>
    /// Facebook Recipient object used as part of a Facebook message.
    /// </summary>
    public class FacebookRecipient
    {
        /// <summary>
        /// Gets or sets the message recipient ID.
        /// </summary>
        /// <value>The Page-scoped ID (PSID) of the message recipient.</value>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the value for the `user_ref` parameter of the checkbox plugin.
        /// </summary>
        /// <value>Used for the checkbox plugin.</value>
        [JsonProperty(PropertyName = "user_ref")]
        public string UserRef { get; set; }

        /// <summary>
        /// Gets or sets the post ID.
        /// </summary>
        /// <value>Used for private replies to reference the visitor post to reply to.</value>
        [JsonProperty(PropertyName = "post_id")]
        public string PostId { get; set; }

        /// <summary>
        /// Gets or sets the comment ID.
        /// </summary>
        /// <value>Used for private replies to reference the visitor comment to reply to.</value>
        [JsonProperty(PropertyName = "comment_id")]
        public string CommentId { get; set; }
    }
}
