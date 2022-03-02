// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema.Teams
{
    /// <summary>
    /// A channel info object which describes the channel.
    /// </summary>
    public class ChannelInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelInfo"/> class.
        /// </summary>
        public ChannelInfo()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelInfo"/> class.
        /// </summary>
        /// <param name="id">Unique identifier representing a channel.</param>
        /// <param name="name">Name of the channel.</param>
        public ChannelInfo(string id = default, string name = default)
        {
            Id = id;
            Name = name;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets unique identifier representing a channel.
        /// </summary>
        /// <value>The channel ID.</value>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets name of the channel.
        /// </summary>
        /// <value>The channel name.</value>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
        }
    }
}
