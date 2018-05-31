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
        /// The connection name that was used
        /// </summary>
        [JsonProperty("connectionName")]
        public string ConnectionName { get; set; }

        /// <summary>
        /// A reference to the conversation
        /// </summary>
        [JsonProperty("conversation")]
        public ConversationReference Conversation { get; set; }

        /// <summary>
        /// The URL of the bot messaging endpoint
        /// </summary>
        [JsonProperty("botUrl")]
        public string BotUrl { get; set; }

        /// <summary>
        /// The bot's registered application ID
        /// </summary>
        [JsonProperty("msAppId")]
        public string MsAppId { get; set; }
    }
}
