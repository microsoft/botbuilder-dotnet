// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary>
    /// Supplementary parameter for media events.
    /// </summary>
    public class MediaEventValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MediaEventValue"/> class.
        /// </summary>
        public MediaEventValue()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaEventValue"/> class.
        /// </summary>
        /// <param name="cardValue">Callback parameter specified in the Value
        /// field of the MediaCard that originated this event.</param>
        public MediaEventValue(object cardValue = default)
        {
            CardValue = cardValue;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets callback parameter specified in the Value field of the
        /// MediaCard that originated this event.
        /// </summary>
        /// <value>The callback parameter specifid in the Value field of the MediaCard that originated this event.</value>
        [JsonPropertyName("cardValue")]
        public object CardValue { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
            throw new System.NotImplementedException();
        }
    }
}
