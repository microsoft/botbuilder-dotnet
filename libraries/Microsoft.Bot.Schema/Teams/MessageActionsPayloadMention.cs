﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the entity that was mentioned in the message.
    /// </summary>
    public class MessageActionsPayloadMention
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageActionsPayloadMention"/> class.
        /// </summary>
        /// <param name="id">The id of the mentioned entity.</param>
        /// <param name="mentionText">The plaintext display name of the
        /// mentioned entity.</param>
        /// <param name="mentioned">Provides more details on the mentioned
        /// entity.</param>
        public MessageActionsPayloadMention(int? id = default, string mentionText = default, MessageActionsPayloadFrom mentioned = default)
        {
            Id = id;
            MentionText = mentionText;
            Mentioned = mentioned;
        }

        /// <summary>
        /// Gets or sets the id of the mentioned entity.
        /// </summary>
        /// <value>The mentioned entity ID.</value>
        [JsonProperty(PropertyName = "id")]
        public int? Id { get; set; }

        /// <summary>
        /// Gets or sets the plaintext display name of the mentioned entity.
        /// </summary>
        /// <value>The plaintext display name of the mentioned entity.</value>
        [JsonProperty(PropertyName = "mentionText")]
        public string MentionText { get; set; }

        /// <summary>
        /// Gets or sets provides more details on the mentioned entity.
        /// </summary>
        /// <value>Details of the mentioned entity.</value>
        [JsonProperty(PropertyName = "mentioned")]
        public MessageActionsPayloadFrom Mentioned { get; set; }
    }
}
