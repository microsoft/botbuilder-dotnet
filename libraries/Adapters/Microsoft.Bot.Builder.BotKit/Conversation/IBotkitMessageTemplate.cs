// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.BotKit.Conversation
{
    /// <summary>
    /// Template for defining a BotkitConversation.
    /// </summary>
    public interface IBotkitMessageTemplate
    {
        /// <summary>
        /// Gets or Sets the Text.
        /// </summary>
        /// <value>Text to the message.</value>
        string[] Text { get; set; }

        /// <summary>
        /// Gets or Sets the Action.
        /// </summary>
        /// <value>Action to be set.</value>
        string Action { get; set; }

        /// <summary>
        /// Gets or sets the Execute.
        /// </summary>
        /// <value>The optional value to execute.</value>
        Execute Execute { get; set; }

        /// <summary>
        /// Gets or Sets the QuickReplies array.
        /// </summary>
        /// <value>The available quick replies.</value>
        object[] QuickReplies { get; set; } // TO-DO: Validate this line

        /// <summary>
        /// Gets or Sets the Attachments array.
        /// </summary>
        /// <value>The attachments to the message.</value>
        object[] Attachments { get; set; }

        /// <summary>
        /// Gets or Sets the ChannelData.
        /// </summary>
        /// <value>
        /// Contains channel-specific content.
        /// </value>
        object ChannelData { get; set; }

        /// <summary>
        /// Gets or Sets the Collect object.
        /// </summary>
        /// <value>
        /// The Collect object.
        /// </value>
        Collect Collect { get; set; }
    }
}
