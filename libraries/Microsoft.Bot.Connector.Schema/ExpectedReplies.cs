// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary>
    /// Replies in response to <see cref="DeliveryModes.ExpectReplies"/>.
    /// </summary>
    public class ExpectedReplies
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpectedReplies"/> class.
        /// </summary>
        public ExpectedReplies()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpectedReplies"/> class.
        /// </summary>
        /// <param name="activities">A collection of Activities that conforms
        /// to the ExpectedReplies schema.</param>
        public ExpectedReplies(IList<Activity> activities = default)
        {
            Activities = activities;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets a collection of Activities that conforms to the
        /// ExpectedReplies schema.
        /// </summary>
        /// <value>The collection of activities that conforms to the ExpectedREplies schema.</value>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Property setter is required for the collection to be deserialized")]
        [JsonPropertyName("activities")]
        public IList<Activity> Activities { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
        }
    }
}
