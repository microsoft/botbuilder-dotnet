﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;

    /// <summary>
    /// O365 connector card fact.
    /// </summary>
    public class O365ConnectorCardFact
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="O365ConnectorCardFact"/> class.
        /// </summary>
        /// <param name="name">Display name of the fact.</param>
        /// <param name="value">Display value for the fact.</param>
        public O365ConnectorCardFact(string name = default, string value = default)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Gets or sets display name of the fact.
        /// </summary>
        /// <value>The name of the fact.</value>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets display value for the fact.
        /// </summary>
        /// <value>The display value for the fact.</value>
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }
    }
}
