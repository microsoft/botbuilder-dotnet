// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;

    /// <summary>
    /// Envelope for Feedback ActionValue Response.
    /// </summary>
    public partial class FeedbackResponseActionValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackResponseActionValue"/> class.
        /// </summary>
        public FeedbackResponseActionValue()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackResponseActionValue"/> class.
        /// </summary>
        /// <param name="reaction">The reaction of the feedback.</param>
        /// <param name="feedback">The feedback content.</param>
        public FeedbackResponseActionValue(string reaction = default, string feedback = default)
        {
            Reaction = reaction;
            Feedback = feedback;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the reaction, either "like" or "dislike".
        /// </summary>
        /// <value>val.</value>
        [JsonProperty(PropertyName = "reaction")]
        public string Reaction { get; set; }

        /// <summary>
        /// Gets or sets the feedback content provided by the user when prompted with "What did you like/dislike?".
        /// </summary>
        /// <value>val.</value>
        [JsonProperty(PropertyName = "feedback")]
        public string Feedback { get; set; }

        partial void CustomInit();
    }
}
