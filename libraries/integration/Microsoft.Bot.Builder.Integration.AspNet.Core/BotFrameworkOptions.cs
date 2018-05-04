// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Rest.TransientFaultHandling;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    /// <summary>
    /// Contains settings that your ASP.NET application uses to initialize the <see cref="BotAdapter"/>
    /// that it adds to the HTTP request pipeline.
    /// </summary>
    /// <seealso cref="ApplicationBuilderExtensions"/>
    public class BotFrameworkOptions
    {
        private readonly List<IMiddleware> _middleware;

        /// <summary>
        /// Creates a <see cref="BotFrameworkOptions"/> object.
        /// </summary>
        public BotFrameworkOptions()
        {
            _middleware = new List<IMiddleware>();
        }

        /// <summary>
        /// An <see cref="ICredentialProvider"/> that should be used to store and retrieve credentials used during authentication with the Bot Framework Service.
        /// </summary>
        public ICredentialProvider CredentialProvider { get; set; }
        
        /// <summary>
        /// A list of <see cref="IMiddleware"/> that will be executed for each turn of the conversation.
        /// </summary>
        public IList<IMiddleware> Middleware { get => _middleware; }

        /// <summary>
        /// Gets or sets whether a proactive messaging endpoint should be exposed for the bot.
        /// </summary>
        /// <value>
        /// True if the proactive messaging endpoint should be enabled, otherwise false.
        /// </value>
        public bool EnableProactiveMessages { get; set; }

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
