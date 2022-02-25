// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Invoke ('tab/submit') request value payload data.
    /// </summary>
    public partial class TabSubmitData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TabSubmitData"/> class.
        /// </summary>
        public TabSubmitData()
        {
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the type for this TabSubmitData.
        /// </summary>
        /// <value>
        /// Currently, 'tab/submit'.
        /// </value>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets properties that are not otherwise defined by the <see cref="TabSubmit"/> type but that
        /// might appear in the serialized REST JSON object.
        /// </summary>
        /// <value>The extended properties for the object.</value>
        /// <remarks>With this, properties not represented in the defined type are not dropped when
        /// the JSON object is deserialized, but are instead stored in this property. Such properties
        /// will be written to a JSON object when the instance is serialized.</remarks>
        [JsonExtensionData(ReadData = true, WriteData = true)]
        public JObject Properties { get; private set; } = new JObject();

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
