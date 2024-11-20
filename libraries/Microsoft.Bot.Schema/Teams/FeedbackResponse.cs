// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;

    /// <summary>
    /// Envelope for Feedback Response.
    /// </summary>
    public partial class FeedbackResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackResponse"/> class.
        /// </summary>
        public FeedbackResponse()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackResponse"/> class.
        /// </summary>
        /// <param name="actionName">Unique identifier representing a team.</param>
        /// <param name="actionValue">Unique identifier representing a team2.</param>
        /// <param name="replyToId">Unique identifier representing a team3.</param>
        public FeedbackResponse(string actionName = default, FeedbackResponseActionValue actionValue = default, string replyToId = default)
        {
            ActionName = actionName;
            ActionValue = actionValue;
            ReplyToId = replyToId;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the action name.
        /// </summary>
        /// <value>Name of the action.</value>
        public string ActionName { get; set; } = "feedback";

        /// <summary>
        /// Gets or sets the response for the action value.
        /// </summary>
        /// <value>The action value that contains the feedback reaction and message.</value>
        [JsonProperty(PropertyName = "actionValue")]
        public FeedbackResponseActionValue ActionValue { get; set; }

        /// <summary>
        /// Gets or sets the ID of the message to which this message is a reply.
        /// </summary>
        /// <value>Value of the ID to reply.</value>
        [JsonProperty(PropertyName = "replyToId")]
        public string ReplyToId { get; set; }

        partial void CustomInit();
    }
}
