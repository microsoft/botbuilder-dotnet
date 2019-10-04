// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents
{
    public class FacebookRecipient
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        /// <value>Page Scoped User ID (PSID) of the message recipient.</value>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the UserRef.
        /// </summary>
        /// <value>Used for the checkbox plugin.</value>
        [JsonProperty(PropertyName = "user_ref")]
        public string UserRef { get; set; }

        /// <summary>
        /// Gets or sets the PostId.
        /// </summary>
        /// <value>Used for Private Replies to reference the visitor post to reply to.</value>
        [JsonProperty(PropertyName = "post_id")]
        public string PostId { get; set; }

        /// <summary>
        /// Gets or sets the CommentId.
        /// </summary>
        /// <value>Used for Private Replies to reference the post comment to reply to.</value>
        [JsonProperty(PropertyName = "comment_id")]
        public string CommentId { get; set; }
    }
}
