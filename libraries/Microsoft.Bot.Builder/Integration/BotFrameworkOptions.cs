// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Serialization;
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
        public ICredentialProvider CredentialProvider { get; set; }

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
        /// Gets or sets a value indicating whether gets or sets whether a proactive messaging endpoint should be exposed for the bot.
        /// </summary>
        /// <value>
        /// True if the proactive messaging endpoint should be enabled, otherwise false.
        /// </value>
        public bool EnableProactiveMessages { get; set; }

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
        /// Gets or sets an <see cref="IActivitySerializer">activity serializer implementation</see> that should be used to serialize <see cref="Activity"/>
        /// that are sent to or received from the bot.
        /// </summary>
        /// <value>An <see cref="IActivitySerializer"/> that will be used for serialization.</value>
        /// <seealso cref="JsonActivitySerializer"/>
        public IActivitySerializer ActivitySerializer { get; set; }
    }
}
