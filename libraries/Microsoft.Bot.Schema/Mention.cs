// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Mention information (entity type: "mention").
    /// </summary>
    public class Mention : Entity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Mention"/> class.
        /// </summary>
        public Mention()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mention"/> class.
        /// </summary>
        /// <param name="mentioned">The mentioned user.</param>
        /// <param name="text">Sub Text which represents the mention (can be
        /// null or empty).</param>
        /// <param name="type">Type of this entity (RFC 3987 IRI).</param>
        public Mention(ChannelAccount mentioned = default(ChannelAccount), string text = default(string), string type = default(string))
        {
            Mentioned = mentioned;
            Text = text;
            Type = type;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the mentioned user.
        /// </summary>
        /// <value>The mentioned user.</value>
        [JsonProperty(PropertyName = "mentioned")]
        public ChannelAccount Mentioned { get; set; }

        /// <summary>
        /// Gets or sets sub Text which represents the mention (can be null or
        /// empty).
        /// </summary>
        /// <value>The sub text with represents the mention.</value>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
            Type = "mention";
        }
    }
}
