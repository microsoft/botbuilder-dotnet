// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema.Teams
{
    /// <summary>
    /// Represents a team or channel entity.
    /// </summary>
    public class MessageActionsPayloadConversation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageActionsPayloadConversation"/> class.
        /// </summary>
        public MessageActionsPayloadConversation()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageActionsPayloadConversation"/> class.
        /// </summary>
        /// <param name="conversationIdentityType">The type of conversation,
        /// whether a team or channel. Possible values include: 'team',
        /// 'channel'.</param>
        /// <param name="id">The id of the team or channel.</param>
        /// <param name="displayName">The plaintext display name of the team or
        /// channel entity.</param>
        public MessageActionsPayloadConversation(string conversationIdentityType = default, string id = default, string displayName = default)
        {
            ConversationIdentityType = conversationIdentityType;
            Id = id;
            DisplayName = displayName;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the type of conversation, whether a team or channel.
        /// Possible values include: 'team', 'channel'.
        /// </summary>
        /// <value>The type of converation, indicating whether it's a team or channel.</value>
        [JsonPropertyName("conversationIdentityType")]
        public string ConversationIdentityType { get; set; }

        /// <summary>
        /// Gets or sets the id of the team or channel.
        /// </summary>
        /// <value>The ID of the team or channel.</value>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the plaintext display name of the team or channel
        /// entity.
        /// </summary>
        /// <value>The plaintext display name of the team or channel.</value>
        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
            throw new System.NotImplementedException();
        }
    }
}
