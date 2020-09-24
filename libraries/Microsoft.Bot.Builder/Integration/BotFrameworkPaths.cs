// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Integration
{
    /// <summary>
    /// A class that defines the bot framework default path values.
    /// </summary>
    public class BotFrameworkPaths
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BotFrameworkPaths"/> class and
        /// sets BasePath and MessagePath to default values.
        /// </summary>
        public BotFrameworkPaths()
        {
            BasePath = "/api";
            MessagesPath = "/messages";
        }

        /// <summary>
        /// Gets or sets the base path at which the bot's endpoints should be exposed.
        /// </summary>
        /// <value>
        /// A string that represents the base URL at which the bot should be exposed.
        /// </value>
        public string BasePath { get; set; }

        /// <summary>
        /// Gets or sets the path, relative to the <see cref="BasePath"/>, at which the bot framework messages are expected to be delivered.
        /// </summary>
        /// <value>
        /// A string that represents the URL at which the bot framework messages are expected to be delivered.
        /// </value>
        public string MessagesPath { get; set; }
    }
}
