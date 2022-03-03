﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a user, application, or conversation type that either sent
    /// or was referenced in a message.
    /// </summary>
    public class MessageActionsPayloadFrom
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageActionsPayloadFrom"/> class.
        /// </summary>
        /// <param name="user">Represents details of the user.</param>
        /// <param name="application">Represents details of the app.</param>
        /// <param name="conversation">Represents details of the
        /// converesation.</param>
        public MessageActionsPayloadFrom(MessageActionsPayloadUser user = default, MessageActionsPayloadApp application = default, MessageActionsPayloadConversation conversation = default)
        {
            User = user;
            Application = application;
            Conversation = conversation;
        }

        /// <summary>
        /// Gets or sets represents details of the user.
        /// </summary>
        /// <value>The user details.</value>
        [JsonProperty(PropertyName = "user")]
        public MessageActionsPayloadUser User { get; set; }

        /// <summary>
        /// Gets or sets represents details of the app.
        /// </summary>
        /// <value>The application details.</value>
        [JsonProperty(PropertyName = "application")]
        public MessageActionsPayloadApp Application { get; set; }

        /// <summary>
        /// Gets or sets represents details of the converesation.
        /// </summary>
        /// <value>The conversation details.</value>
        [JsonProperty(PropertyName = "conversation")]
        public MessageActionsPayloadConversation Conversation { get; set; }
    }
}
