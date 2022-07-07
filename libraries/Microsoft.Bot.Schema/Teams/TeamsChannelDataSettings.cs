// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Settings within teams channel data specific to messages received in Microsoft Teams.
    /// </summary>
    public partial class TeamsChannelDataSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsChannelDataSettings"/> class.
        /// </summary>
        public TeamsChannelDataSettings()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsChannelDataSettings"/> class.
        /// </summary>
        /// <param name="channel">Information about the channel in which the message was sent.</param>
        public TeamsChannelDataSettings(ChannelInfo channel = default)
        {
            SelectedChannel = channel;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets information about the selected Teams channel.
        /// </summary>
        /// <value>The selected Teams channel.</value>
        [JsonProperty(PropertyName = "selectedChannel")]
        public ChannelInfo SelectedChannel { get; set; }

        /// <summary>
        /// Gets or sets properties that are not otherwise defined by the <see cref="TeamsChannelDataSettings"/> type but that
        /// might appear in the REST JSON object.
        /// </summary>
        /// <value>The extended properties for the object.</value>
        /// <remarks>With this, properties not represented in the defined type are not dropped when
        /// the JSON object is deserialized, but are instead stored in this property. Such properties
        /// will be written to a JSON object when the instance is serialized.</remarks>
        [JsonExtensionData(ReadData = true, WriteData = true)]
#pragma warning disable CA2227 // Collection properties should be read only
        public IDictionary<string, object> AdditionalProperties { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
