// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Runtime.CompilerServices;
using Microsoft.Bot.Builder.Adapters;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.UserTokenMocks
{
    /// <summary>
    /// Mock UserToken with just user id and token.
    /// </summary>
    public class UserTokenBasicMock : UserTokenMock
    {
        /// <summary>
        /// The kind for this class.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.Test.UserTokenBasicMock";

        /// <summary>
        /// Initializes a new instance of the <see cref="UserTokenBasicMock"/> class.
        /// </summary>
        /// <param name="path">optional path.</param>
        /// <param name="line">optional line.</param>
        [JsonConstructor]
        public UserTokenBasicMock([CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
        {
            RegisterSourcePath(path, line);
        }

        /// <summary>
        /// Gets or sets the connection name.
        /// </summary>
        /// <value>
        /// The connection name.
        /// </value>
        [JsonProperty("connectionName")]
        public string ConnectionName { get; set; }

        /// <summary>
        /// Gets or sets the channel ID.
        /// </summary>
        /// <value>
        /// The channel ID. If empty, same as adapter.Conversation.ChannelId.
        /// </value>
        [JsonProperty("channelId")]
        public string ChannelId { get; set; }

        /// <summary>
        /// Gets or sets the user ID.
        /// </summary>
        /// <value>
        /// The user ID. If empty, same as adapter.Conversation.User.Id.
        /// </value>
        [JsonProperty("userId")]
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the token to store.
        /// </summary>
        /// <value>
        /// The token to store.
        /// </value>
        [JsonProperty("token")]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the optional magic code to associate with this token.
        /// </summary>
        /// <value>
        /// The optional magic code to associate with this token.
        /// </value>
        [JsonProperty("magicCode")]
        public string MagicCode { get; set; }

        /// <inheritdoc/>
        public override void Setup(TestAdapter adapter)
        {
            var conversation = adapter.Conversation;
            var channelId = string.IsNullOrEmpty(ChannelId) ? conversation.ChannelId : ChannelId;
            var userId = string.IsNullOrEmpty(UserId) ? conversation.User.Id : UserId;
            adapter.AddUserToken(ConnectionName, channelId, userId, Token, MagicCode);
        }
    }
}
