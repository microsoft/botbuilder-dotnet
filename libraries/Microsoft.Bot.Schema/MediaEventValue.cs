// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using Newtonsoft.Json;

    /// <summary>
    /// Supplementary parameter for media events.
    /// </summary>
    public class MediaEventValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MediaEventValue"/> class.
        /// </summary>
        /// <param name="cardValue">Callback parameter specified in the Value
        /// field of the MediaCard that originated this event.</param>
        public MediaEventValue(object cardValue = default)
        {
            CardValue = cardValue;
        }

        /// <summary>
        /// Gets or sets callback parameter specified in the Value field of the
        /// MediaCard that originated this event.
        /// </summary>
        /// <value>The callback parameter specifid in the Value field of the MediaCard that originated this event.</value>
        [JsonProperty(PropertyName = "cardValue")]
        public object CardValue { get; set; }
    }
}
