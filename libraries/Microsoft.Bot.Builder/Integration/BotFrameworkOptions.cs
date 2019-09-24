// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Rest.TransientFaultHandling;

namespace Microsoft.Bot.Builder.Integration
{
    /// <summary>
    /// Contains settings used by the .NET integration APIs to initialize the <see cref="BotFrameworkAdapter"/>
    /// that processes the HTTP requests coming from the Bot Framework Service.
    /// </summary>
    public class BotFrameworkOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BotFrameworkOptions"/> class.
        /// </summary>
        public BotFrameworkOptions()
        {
        }

        /// <summary>
        /// Gets or sets an <see cref="ICredentialProvider"/> that should be used to store and retrieve the
        /// credentials used during authentication with the Bot Framework Service.
        /// </summary>
        /// <value>The credential provider.</value>
        public ICredentialProvider CredentialProvider { get; set; } = new SimpleCredentialProvider();

        /// <summary>
        /// Gets or sets an <see cref="IChannelProvider"/> that should be used to provide configuration for
        /// how to validate authentication tokens received from the Bot Framework Service.
        /// </summary>
        /// <value>The credential provider.</value>
        public IChannelProvider ChannelProvider { get; set; }

        /// <summary>
        /// Gets or sets an error handler to use to catch exceptions in the middleware or application.
        /// </summary>
        /// <value>The error handler.</value>
        public Func<ITurnContext, Exception, Task> OnTurnError { get; set; }

        /// <summary>
        /// Gets a list of the <see cref="IMiddleware"/> to use on each incoming activity.
        /// </summary>
        /// <value>The middleware list.</value>
        /// <seealso cref="BotAdapter.Use(IMiddleware)"/>
        public IList<IMiddleware> Middleware { get; } = new List<IMiddleware>();

#pragma warning disable SA1623, SA1515, SA1516
        /// <summary>
        /// OBSOLETE: This property is no longer used by the framework.
        /// </summary>
        /// <remarks>
        /// This property was used in a pattern to propagate <see cref="BotState"/> instances
        /// throughout the service configuration phase, but it was never used at runtime and is
        /// being retired to reduce the surface area of state APIs. As an alternative, consider
        /// simply using an appropriately scoped <see cref="BotState"/> variable along with closures.
        /// </remarks>
        /// <value>The list of property state providers.</value>
        [Obsolete("This property is no longer used by the framework. Please see documentation for more details.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IList<BotState> State { get; } = new List<BotState>();
#pragma warning restore SA1623, SA1515, SA1516

        /// <summary>
        /// Gets or sets the retry policy to use in case of errors from Bot Framework Service.
        /// </summary>
        /// <value>The retry policy.</value>
        public RetryPolicy ConnectorClientRetryPolicy { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="HttpClient"/> instance that should be used to make requests to the Bot Framework Service.
        /// </summary>
        /// <value>The HTTP client.</value>
        public HttpClient HttpClient { get; set; }

        /// <summary>
        /// Gets or sets what paths should be used when exposing the various bot endpoints.
        /// </summary>
        /// <value>The path strings.</value>
        /// <seealso cref="BotFrameworkPaths"/>
        public BotFrameworkPaths Paths { get; set; } = new BotFrameworkPaths();

        /// <summary>
        /// Gets or sets the general configuration settings for authentication.
        /// </summary>
        /// <seealso cref="AuthenticationConfiguration"/>
        /// <value>
        /// The general configuration settings for authentication.
        /// </value>
        public AuthenticationConfiguration AuthenticationConfiguration { get; set; } = new AuthenticationConfiguration();
    }
}
