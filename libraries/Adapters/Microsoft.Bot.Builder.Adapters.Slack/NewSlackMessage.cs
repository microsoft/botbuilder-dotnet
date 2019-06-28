// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using SlackAPI.WebSocketMessages;

namespace Microsoft.BotBuilder.Adapters.Slack
{
    /// <summary>
    /// Message to send to Slack.
    /// </summary>
    public class NewSlackMessage : NewMessage
    {
        /// <summary>
        /// Gets or Sets the message as ephemeral, it means, only the recipient can see it.
        /// </summary>
        public string Ephemeral { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a message is sent by the user or not.
        /// </summary>
        public bool AsUser { get; set; }

        /// <summary>
        /// Gets or Sets the URL for an icon.
        /// </summary>
        public string IconUrl { get; set; }

        /// <summary>
        /// Gets or Sets an emoji icon.
        /// </summary>
        public string IconEmoji { get; set; }

        /// <summary>
        /// Gets or Sets the timestamp of the thread.
        /// </summary>
        public string ThreadTS { get; set; }
    }
}
