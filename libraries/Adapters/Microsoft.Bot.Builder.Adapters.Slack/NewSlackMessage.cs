// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using SlackAPI;
using SlackAPI.WebSocketMessages;

namespace Microsoft.Bot.Builder.Adapters.Slack
{
    /// <summary>
    /// Message to send to Slack.
    /// </summary>
    public class NewSlackMessage
    {
        /// <summary>
        /// Gets or Sets the message as ephemeral, it means, only the recipient can see it.
        /// </summary>
        /// <value>The ephemeral indicator of the message.</value>
        public string Ephemeral { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a message is sent by the user or not.
        /// </summary>
        /// <value>The asuser indicator of the message.</value>
        public bool AsUser { get; set; }

        /// <summary>
        /// Gets or Sets the URL for an icon.
        /// </summary>
        /// <value>The URL for an icon.</value>
        public Uri IconUrl { get; set; }

        /// <summary>
        /// Gets or Sets an emoji icon.
        /// </summary>
        /// <value>The emoji icon.</value>
        public string IconEmoji { get; set; }

        /// <summary>
        /// Gets or Sets the timestamp of the thread.
        /// </summary>
        /// <value>Provide another message's timestamp value to make this message a reply.</value>
        public string ThreadTS { get; set; }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        /// <value>The user who sent the message.</value>
        public string User { get; set; }

        /// <summary>
        /// Gets or sets the channel.
        /// </summary>
        /// <value>The channel, private group, or IM channel to send message to.</value>
        public string Channel { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text of the message.</value>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the team.
        /// </summary>
        /// <value>The team the user belongs to.</value>
        public string Team { get; set; }

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        /// <value>The timestamp for the message.</value>
        public string TS { get; set; }

        /// <summary>
        /// Gets or sets the thread ts.
        /// </summary>
        /// <value>The ID of another un-threaded message to reply to.</value>
        public string ThreadTs { get; set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>Your bot's user name. Must be used in conjunction with as_user set to false, otherwise ignored.</value>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the bot id.
        /// </summary>
        /// <value>The bot id.</value>
        public string BotId { get; set; }

        /// <summary>
        /// Gets or sets the icons.
        /// </summary>
        /// <value>A set of icons that could come with the message.</value>
        public UserProfile Icons { get; set; }

        /// <summary>
        /// Gets or sets the blocks that could come with the message.
        /// </summary>
        /// <value>The blocks that could come with the message.</value>
        public List<Block> Blocks { get; set; }

        /// <summary>
        /// Gets or sets the attachments that could come with the message.
        /// </summary>
        /// <value>The attachments that could come with the message.</value>
        public List<Attachment> Attachments { get; set; }
    }
}
