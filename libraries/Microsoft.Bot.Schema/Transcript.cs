// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Transcript.
    /// </summary>
    public partial class Transcript
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Transcript"/> class.
        /// </summary>
        public Transcript()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Transcript"/> class.
        /// </summary>
        /// <param name="activities">A collection of Activities that conforms
        /// to the Transcript schema.</param>
        public Transcript(IList<Activity> activities = default(IList<Activity>))
        {
            Activities = activities;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets a collection of Activities that conforms to the
        /// Transcript schema.
        /// </summary>
        /// <value>A collection of activities that conforms to the Transcript schema.</value>
        [JsonProperty(PropertyName = "activities")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking compat).
        public IList<Activity> Activities { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
