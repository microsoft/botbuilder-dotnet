// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
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
        /// The credential provider with which to initialize the adapter.
        /// </summary>
        public ICredentialProvider CredentialProvider { get; set; }

        /// <summary>
        /// The middleware collection with which to initialize the adapter.
        /// </summary>
        public IList<IMiddleware> Middleware { get => _middleware; }

        /// <summary>
        /// Indicates whether to enable the proactive messages endpoint for the bot.
        /// </summary>
        public bool EnableProactiveMessages { get; set; }

        /// <summary>
        /// Gets or sets the retry policy to retry operations in case of errors from Bot Framework.
        /// </summary>
        public RetryPolicy ConnectorClientRetryPolicy { get; set; }
    }
}
