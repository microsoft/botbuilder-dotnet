// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// SharePoint Ace Data object.
    /// </summary>
    public class AceData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AceData"/> class.
        /// </summary>
        public AceData()
        {
            // Do nothing
        }

        /// <summary>
        /// This enum contains the different types of card templates available in the SPFx framework.
        /// </summary>
        public enum AceCardSize
        {
            /// <summary>
            /// Medium
            /// </summary>
            Medium,

            /// <summary>
            /// Large
            /// </summary>
            Large
        }

        /// <summary>
        /// Gets or Sets the card size of the adaptive card extension of type <see cref="AceCardSize"/> enum.
        /// </summary>
        /// <value>This value is the size of the adaptive card extension.</value>
        [JsonProperty(PropertyName = "cardSize")]
        [JsonConverter(typeof(StringEnumConverter))]
        public AceCardSize CardSize { get; set; }

        /// <summary>
        /// Gets or Sets the version of the data of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the version of the adaptive card extension.</value>
        [JsonProperty(PropertyName = "dataVersion")]
        public string DataVersion { get; set; }

        /// <summary>
        /// Gets or Sets the id of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the ID of the adaptive card extension.</value>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or Sets the title of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the title of the adaptive card extension.</value>
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or Sets the description of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the description of the adaptive card extension.</value>
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or Sets the icon property of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the icon of the adaptive card extension.</value>
        [JsonProperty(PropertyName = "iconProperty")]
        public string IconProperty { get; set; }

        /// <summary>
        /// Gets or Sets the property bag of type <see cref="Uri"/>.
        /// </summary>
        /// <value>This value is the property bag of the adaptive card extension.</value>
        [JsonProperty(PropertyName = "properties")]
#pragma warning disable CA2227
        public JObject Properties { get; set; }
    }
}
