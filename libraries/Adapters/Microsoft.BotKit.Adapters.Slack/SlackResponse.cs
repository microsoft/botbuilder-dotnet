// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.BotKit.Adapters.Slack
{
    /// <summary>
    /// SlackResponse class.
    /// </summary>
    public class SlackResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether the Ok status is true or false.
        /// </summary>
        public bool Ok { get; set; }

        /// <summary>
        /// Gets or sets the Channel property.
        /// </summary>
        public string Channel { get; set; }

        /// <summary>
        /// Gets or sets the TS property.
        /// </summary>
        public string TS { get; set; }

        /// <summary>
        /// Gets or sets the Message property.
        /// </summary>
        public SlackMessage Message { get; set; }
    }
}
