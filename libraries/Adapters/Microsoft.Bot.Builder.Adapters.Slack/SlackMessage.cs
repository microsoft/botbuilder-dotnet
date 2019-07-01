// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.BotBuilder.Adapters.Slack
{
    /// <summary>
    /// SlackMessage class.
    /// </summary>
    public class SlackMessage
    {
        /// <summary>
        /// Gets or sets the Type property.
        /// </summary>
        /// <value>The type of message.</value>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the Subtype property.
        /// </summary>
        /// <value>The subtype of message.</value>
        public string Subtype { get; set; }

        /// <summary>
        /// Gets or sets the Text property.
        /// </summary>
        /// <value>The text of the message.</value>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the TS property.
        /// </summary>
        /// <value>The timestamp of the message.</value>
        public string TS { get; set; }

        /// <summary>
        /// Gets or sets the Username property.
        /// </summary>
        /// <value>The name of the user that sends the message.</value>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the BotID property.
        /// </summary>
        /// <value>The botId of the message.</value>
        public string BotID { get; set; }

        /// <summary>
        /// Gets or sets the ThreadTS property.
        /// </summary>
        /// <value>The thread timestamp of the message.</value>
        public string ThreadTS { get; set; }
    }
}
