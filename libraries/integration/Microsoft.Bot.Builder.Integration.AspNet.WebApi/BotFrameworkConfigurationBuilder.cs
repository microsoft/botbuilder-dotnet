// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using System;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Rest.TransientFaultHandling;
using System.Net.Http;

namespace Microsoft.Bot.Builder.Integration.AspNet.WebApi
{
    public class BotFrameworkConfigurationBuilder
    {
        private BotFrameworkOptions _options;

        public BotFrameworkConfigurationBuilder(BotFrameworkOptions botFrameworkOptions)
        {
            _options = botFrameworkOptions;
        }

        public BotFrameworkOptions BotFrameworkOptions { get => _options; }

        /// <summary>
        /// Configures an <see cref="ICredentialProvider"/> that should be used to store and retrieve credentials used during authentication with the Bot Framework.
        /// </summary>
        /// <param name="credentialProvider">An <see cref="ICredentialProvider"/> that the bot framework will use to authenticate requests.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <seealso cref="ICredentialProvider" />
        public BotFrameworkConfigurationBuilder UseCredentialProvider(ICredentialProvider credentialProvider)
        {
            _options.CredentialProvider = credentialProvider;

            return this;
        }

        /// <summary>
        /// Adds a piece of <see cref="IMiddleware"/> to the bot's middleware pipeline.
        /// </summary>
        /// <param name="middleware">An instance of <see cref="IMiddleware">middleware</see> that should be added to the bot's middleware pipeline.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <seealso cref="IMiddleware"/>
        public BotFrameworkConfigurationBuilder UseMiddleware(IMiddleware middleware)
        {
            _options.Middleware.Add(middleware);

            return this;
        }

        /// <summary>
        /// Adds retry policy on failure for Bot Framework Service calls.
        /// </summary>
        /// <param name="retryPolicy">The retry policy.</param>
        /// <returns><see cref="BotFrameworkConfigurationBuilder"/> instance with the retry policy set.</returns>
        public BotFrameworkConfigurationBuilder UseRetryPolicy(RetryPolicy retryPolicy)
        {
            _options.ConnectorClientRetryPolicy = retryPolicy;
            return this;
        }

        /// <summary>
        /// Sets the <see cref="HttpClient"/> instance that will be used to make Bot Framework Service calls.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/> to be used when calling the Bot Framework Service.</param>
        /// <returns><see cref="BotFrameworkConfigurationBuilder"/> instance with the <see cref="HttpClient"/> set.</returns>
        public BotFrameworkConfigurationBuilder UseHttpClient(HttpClient httpClient)
        {
            _options.HttpClient = httpClient;

            return this;
        }

        /// <summary>
        /// Enables the proactive messaging endpoint and optionally allows specifying the path at which the endpoint should be exposed.
        /// </summary>
        /// <param name="proactiveMessagesPath">The path at which the proactive messaging endpoint should be exposed.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <seealso cref="BotFrameworkPaths"/>
        /// <seealso cref="UsePaths(Action{BotFrameworkPaths})"/>
        public BotFrameworkConfigurationBuilder EnableProactiveMessages(string proactiveMessagesPath = default(string))
        {
            _options.EnableProactiveMessages = true;
            
            if (proactiveMessagesPath != null)
            
{
                _options.Paths.ProactiveMessagesPath = proactiveMessagesPath;
            }

            return this;
        }

        /// <summary>
        /// Configures which paths should be used to expose the various endpoints of the bot.
        /// </summary>
        /// <param name="configurePaths">A callback to configure the paths that determine where the endpoints of the bot will be exposed.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <seealso cref="BotFrameworkPaths"/>
        public BotFrameworkConfigurationBuilder UsePaths(Action<BotFrameworkPaths> configurePaths)
        {
            configurePaths(_options.Paths);

            return this;
        }
    }

    public static class BotFrameworkConfigurationBuilderExtensions
    {
        /// <summary>
        /// Configures the bot with the a single identity that will be used to authenticate requests made to the Bot Framework.
        /// </summary>
        /// <param name="builder">The <see cref="BotFrameworkConfigurationBuilder"/></param>
        /// <param name="applicationId">The application id that should be used to authenticate requests made to the Bot Framework.</param>
        /// <param name="applicationPassword">The application password that should be used to authenticate requests made to the Bot Framework.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <seealso cref="ICredentialProvider"/>
        /// <seealso cref="SimpleCredentialProvider"/>
        public static BotFrameworkConfigurationBuilder UseMicrosoftApplicationIdentity(this BotFrameworkConfigurationBuilder builder, string applicationId, string applicationPassword) =>
            builder.UseCredentialProvider(new SimpleCredentialProvider(applicationId, applicationPassword));

    }
}