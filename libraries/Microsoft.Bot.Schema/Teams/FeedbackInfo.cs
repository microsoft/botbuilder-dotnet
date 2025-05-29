// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;

    /// <summary>
    /// Describes feedback loop information.
    /// </summary>
    public partial class FeedbackInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackInfo"/> class.
        /// </summary>
        public FeedbackInfo()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackInfo"/> class.
        /// </summary>
        /// <param name="type">Unique identifier representing a team.</param>
        public FeedbackInfo(string type = FeedbackInfoTypes.Default)
        {
            Type = type;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the feedback loop type. Possible values include: 'default', 'custom'.
        /// </summary>
        /// <value>
        /// The feedback loop type (see <see cref="FeedbackInfoTypes"/>). 
        /// </value>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        partial void CustomInit();
    }
}
