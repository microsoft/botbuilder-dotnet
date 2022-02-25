// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// State object passed to the bot token service.
    /// </summary>
    public class TokenExchangeState
    {
        /// <summary>
        /// Gets or sets the connection name that was used.
        /// </summary>
        /// <value>
        /// The connection name that was used.
        /// </value>
        [JsonProperty("connectionName")]
        public string ConnectionName { get; set; }

        /// <summary>
        /// Gets or sets a reference to the conversation.
        /// </summary>
        /// <value>
        /// A reference to the conversation.
        /// </value>
        [JsonProperty("conversation")]
        public ConversationReference Conversation { get; set; }

        /// <summary>
        /// Gets or sets a reference to a related parent conversation for this token exchange.
        /// </summary>
        /// <value>
        /// A reference to a related parent conversation conversation.
        /// </value>
        [JsonProperty("relatesTo")]
        public ConversationReference RelatesTo { get; set; }

        /// <summary>
        /// Gets or sets the URL of the bot messaging endpoint.
        /// </summary>
        /// <value>
        /// The URL of the bot messaging endpoint.
        /// </value>
        [JsonProperty("botUrl")]
#pragma warning disable CA1056 // Uri properties should not be strings (we can't change this without breaking binary compat)
        public string BotUrl { get; set; }
#pragma warning restore CA1056 // Uri properties should not be strings

        /// <summary>
        /// Gets or sets the bot's registered application ID.
        /// </summary>
        /// <value>
        /// The bot's registered application ID.
        /// </value>
        [JsonProperty("msAppId")]
        public string MsAppId { get; set; }
    }
}
