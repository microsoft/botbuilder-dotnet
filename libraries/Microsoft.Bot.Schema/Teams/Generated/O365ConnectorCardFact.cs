// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// O365 connector card fact
    /// </summary>
    public partial class O365ConnectorCardFact
    {
        /// <summary>
        /// Initializes a new instance of the O365ConnectorCardFact class.
        /// </summary>
        public O365ConnectorCardFact()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the O365ConnectorCardFact class.
        /// </summary>
        /// <param name="name">Display name of the fact</param>
        /// <param name="value">Display value for the fact</param>
        public O365ConnectorCardFact(string name = default(string), string value = default(string))
        {
            Name = name;
            Value = value;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets display name of the fact
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets display value for the fact
        /// </summary>
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

    }
}
