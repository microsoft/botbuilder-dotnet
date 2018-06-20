// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    public class BotFrameworkPaths
    {
        public BotFrameworkPaths()
        {
            BasePath = "/api";
            MessagesPath = "/messages";
            ProactiveMessagesPath = "/messages/proactive";
        }

        /// <summary>
        /// Gets or sets the base path at which the bot's endpoints should be exposed.
        /// </summary>
        /// <value>
        /// A <see cref="PathString"/> that represents the base URL at which the bot should be exposed.
        /// </value>
        public PathString BasePath { get; set; }

        /// <summary>
        /// Gets or sets the path, relative to the <see cref="BasePath"/>, at which the bot framework messages are expected to be delivered.
        /// </summary>
        /// <value>
        /// A <see cref="PathString"/> that represents the URL at which the bot framework messages are expected to be delivered.
        /// </value>
        public PathString MessagesPath { get; set; }

        /// <summary>
        /// Gets or sets the path, relative to the <see cref="BasePath"/>, at which proactive messages are expected to be delivered.
        /// </summary>
        /// <value>
        /// A <see cref="PathString"/> that represents the base URL at which proactive messages.
        /// </value>
        /// <remarks>
        /// This path is only utilized if <see cref="BotFrameworkOptions.EnableProactiveMessages">the proactive messaging feature has been enabled</see>.
        /// </remarks>
        /// <seealso cref="BotFrameworkOptions.EnableProactiveMessages"/>
        public PathString ProactiveMessagesPath { get; set; }
    }
}