// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Transcript.
    /// </summary>
    public class Transcript
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Transcript"/> class.
        /// </summary>
        /// <param name="activities">A collection of Activities that conforms
        /// to the Transcript schema.</param>
        public Transcript(IList<Activity> activities = default)
        {
            Activities = activities ?? new List<Activity>();
        }

        /// <summary>
        /// Gets a collection of Activities that conforms to the
        /// Transcript schema.
        /// </summary>
        /// <value>A collection of activities that conforms to the Transcript schema.</value>
        [JsonProperty(PropertyName = "activities")]
        public IList<Activity> Activities { get; private set; } = new List<Activity>();
    }
}
