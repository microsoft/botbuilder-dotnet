// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema
{
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
        public Mention(ChannelAccount mentioned = default, string text = default, string type = default)
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
        [JsonPropertyName("mentioned")]
        public ChannelAccount Mentioned { get; set; }

        /// <summary>
        /// Gets or sets sub Text which represents the mention (can be null or
        /// empty).
        /// </summary>
        /// <value>The sub text with represents the mention.</value>
        [JsonPropertyName("text")]
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
