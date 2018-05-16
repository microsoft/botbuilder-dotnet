// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Rest.TransientFaultHandling;
using System.Collections.Generic;
using System.Net.Http;

namespace Microsoft.Bot.Builder.Integration.AspNet.WebApi
{
    public class BotFrameworkOptions
    {
        private readonly List<IMiddleware> _middleware;
        private readonly BotFrameworkPaths _paths;

        public BotFrameworkOptions()
        {
            _middleware = new List<IMiddleware>();
            _paths = new BotFrameworkPaths();
        }

        /// <summary>
        /// An <see cref="ICredentialProvider"/> that should be used to store and retrieve credentials used during authentication with the Bot Framework Service.
        /// </summary>
        public ICredentialProvider CredentialProvider { get; set; }

        /// <summary>
        /// A list of <see cref="IMiddleware"/> that will be executed for each turn of the conversation.
        /// </summary>
        public List<IMiddleware> Middleware { get => _middleware; }

        /// <summary>
        /// Gets or sets whether a proactive messaging endpoint should be exposed for the bot.
        /// </summary>
        /// <value>
        /// True if the proactive messaging endpoint should be enabled, otherwise false.
        /// </value>
        public bool EnableProactiveMessages { get; set; }

        /// <summary>
        /// Gets or sets what paths should be used when exposing the various bot endpoints.
        /// </summary>
        /// <seealso cref="BotFrameworkPaths" />
        public BotFrameworkPaths Paths { get => _paths; }

        /// <summary>
        /// Gets or sets the retry policy to retry operations in case of errors from Bot Framework Service.
        /// </summary>
        public RetryPolicy ConnectorClientRetryPolicy { get; set; }


        /// <summary>
        /// Gets or sets the <see cref="HttpClient"/> instance that should be used to make requests to the Bot Framework Service.
        /// </summary>
        public HttpClient HttpClient { get; set; }
    }
}