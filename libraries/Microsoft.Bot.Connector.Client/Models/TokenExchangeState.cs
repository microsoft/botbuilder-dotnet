// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Client.Models
{
    /// <summary>
    /// State object passed to the bot token service.
    /// </summary>
    internal class TokenExchangeState
    {
        /// <summary>
        /// Gets or sets the connection name that was used.
        /// </summary>
        /// <value>
        /// The connection name that was used.
        /// </value>
        [JsonPropertyName("connectionName")]
        public string ConnectionName { get; set; }

        /// <summary>
        /// Gets or sets a reference to the conversation.
        /// </summary>
        /// <value>
        /// A reference to the conversation.
        /// </value>
        [JsonPropertyName("conversation")]
        public ConversationReference Conversation { get; set; }

        /// <summary>
        /// Gets or sets a reference to a related parent conversation for this token exchange.
        /// </summary>
        /// <value>
        /// A reference to a related parent conversation conversation.
        /// </value>
        [JsonPropertyName("relatesTo")]
        public ConversationReference RelatesTo { get; set; }

        /// <summary>
        /// Gets or sets the bot's registered application ID.
        /// </summary>
        /// <value>
        /// The bot's registered application ID.
        /// </value>
        [JsonPropertyName("msAppId")]
        public string MicrosoftAppId { get; set; }
    }
}
