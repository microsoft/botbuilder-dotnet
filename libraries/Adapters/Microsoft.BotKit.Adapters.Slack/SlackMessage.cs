// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.BotKit.Adapters.Slack
{
    /// <summary>
    /// SlackMessage class.
    /// </summary>
    public class SlackMessage
    {
        /// <summary>
        /// Gets or sets the Type property.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the Subtype property.
        /// </summary>
        public string Subtype { get; set; }

        /// <summary>
        /// Gets or sets the Text property.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the TS property.
        /// </summary>
        public string TS { get; set; }

        /// <summary>
        /// Gets or sets the Username property.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the BotID property.
        /// </summary>
        public string BotID { get; set; }

        /// <summary>
        /// Gets or sets the ThreadTS property.
        /// </summary>
        public string ThreadTS { get; set; }
    }
}
