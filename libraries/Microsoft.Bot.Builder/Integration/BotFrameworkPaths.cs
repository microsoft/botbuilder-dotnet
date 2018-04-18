// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Integration
{
    public class BotFrameworkPaths
    {
        public BotFrameworkPaths()
        {
            BasePath = "/api";
            MessagesPath = "/messages";
            ExternalEventsPath = "/events";
        }

        /// <summary>
        /// Gets or sets the base path at which the bot's endpoints should be exposed.
        /// </summary>
        /// <value>
        /// A path that represents the base URL at which the bot should be exposed. The default is: <code>api/</code>
        /// </value>
        public string BasePath { get; set; }

        /// <summary>
        /// Gets or sets the path, relative to the <see cref="BasePath"/>, at which the bot framework messages are expected to be delivered.
        /// </summary>
        /// <value>
        /// A path that represents the URL at which the bot framework messages are expected to be delivered. The default is: <code>&lt;base-path&gt;/messages</code>
        /// </value>
        public string MessagesPath { get; set; }

        /// <summary>
        /// Gets or sets the path, relative to the <see cref="BasePath"/>, at which external events are expected to be delivered.
        /// </summary>
        /// <value>
        /// A path that represents the URL at which external events will be received. The default is: <code>&lt;base-path&gt;/events</code>
        /// </value>
        /// <remarks>
        /// This path is only utilized if <see cref="BotFrameworkOptions.EnableExternalEventsEndpoint">the external events endpoint has been enabled</see>.
        /// </remarks>
        /// <seealso cref="BotFrameworkOptions.EnableExternalEventsEndpoint"/>
        /// <seealso cref="BotFrameworkConfigurationBuilder.EnableExternalEventsEndpoint(string)" />
        public string ExternalEventsPath { get; set; }
    }
}