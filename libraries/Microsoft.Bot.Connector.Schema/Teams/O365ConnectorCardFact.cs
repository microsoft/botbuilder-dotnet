// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema.Teams
{
    /// <summary>
    /// O365 connector card fact.
    /// </summary>
    public class O365ConnectorCardFact
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="O365ConnectorCardFact"/> class.
        /// </summary>
        public O365ConnectorCardFact()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="O365ConnectorCardFact"/> class.
        /// </summary>
        /// <param name="name">Display name of the fact.</param>
        /// <param name="value">Display value for the fact.</param>
        public O365ConnectorCardFact(string name = default, string value = default)
        {
            Name = name;
            Value = value;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets display name of the fact.
        /// </summary>
        /// <value>The name of the fact.</value>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets display value for the fact.
        /// </summary>
        /// <value>The display value for the fact.</value>
        [JsonPropertyName("value")]
        public string Value { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
        }
    }
}
