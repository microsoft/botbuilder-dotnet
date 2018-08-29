// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Rest.TransientFaultHandling;

namespace Microsoft.Bot.Builder.Integration
{
    /// <summary>
    /// Contains settings that your ASP.NET application uses to initialize the <see cref="BotAdapter"/>
    /// that it adds to the HTTP request pipeline.
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
        /// Gets or sets an error handler to use to catche exceptions in the middleware or application.
        /// </summary>
        /// <value>The error handler.</value>
        public Func<ITurnContext, Exception, Task> OnTurnError { get; set; }

        /// <summary>
        /// Gets a list of the <see cref="IMiddleware"/> to use on each incoming activity.
        /// </summary>
        /// <value>The middleware list.</value>
        /// <seealso cref="BotAdapter.Use(IMiddleware)"/>
        public IList<IMiddleware> Middleware { get; } = new List<IMiddleware>();

        /// <summary>
        /// Gets a list of the <see cref="BotState"/> providers to use on each incoming activity.
        /// Objects in the State list enable other components to get access to the state providers
        /// during the start up process.  For example, creating state property accessors within a ASP.net Core Singleton
        /// that could be passed to your IBot-derived class.
        /// The providers in this list are not associated with the BotStateSet Middleware component. To clarify, state providers
        /// in this list are not automatically loaded or saved during the turn process.
        /// </summary>
        /// <value>The list of property state providers.</value>
        public IList<BotState> State { get; } = new List<BotState>();

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
    }
}
