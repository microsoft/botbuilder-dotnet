﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// Mention information (entity type: "mention")
    /// </summary>
    public partial class Mention
    {
        /// <summary>
        /// Initializes a new instance of the Mention class.
        /// </summary>
        public Mention()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the Mention class.
        /// </summary>
        /// <param name="mentioned">The mentioned user</param>
        /// <param name="text">Sub Text which represents the mention (can be
        /// null or empty)</param>
        /// <param name="type">Type of this entity (RFC 3987 IRI)</param>
        public Mention(ChannelAccount mentioned = default(ChannelAccount), string text = default(string), string type = default(string))
        {
            Mentioned = mentioned;
            Text = text;
            Type = type;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets the mentioned user
        /// </summary>
        [JsonProperty(PropertyName = "mentioned")]
        public ChannelAccount Mentioned { get; set; }

        /// <summary>
        /// Gets or sets sub Text which represents the mention (can be null or
        /// empty)
        /// </summary>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }
    }
}
