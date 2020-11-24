// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// A channel info object which describes the channel.
    /// </summary>
    public partial class ChannelInfo
    {
        /// <summary>
        /// Initializes a new instance of the ChannelInfo class.
        /// </summary>
        public ChannelInfo()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the ChannelInfo class.
        /// </summary>
        /// <param name="id">Unique identifier representing a channel</param>
        /// <param name="name">Name of the channel</param>
        public ChannelInfo(string id = default(string), string name = default(string))
        {
            Id = id;
            Name = name;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets unique identifier representing a channel
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets name of the channel
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

    }
}
