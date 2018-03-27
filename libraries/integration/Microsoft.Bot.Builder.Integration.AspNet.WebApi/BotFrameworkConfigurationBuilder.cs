// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using System;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Rest.TransientFaultHandling;

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

        public BotFrameworkConfigurationBuilder UseMicrosoftApplicationIdentity(string applicationId, string applicationPassword) =>
            UseCredentialProvider(new SimpleCredentialProvider(applicationId, applicationPassword));

        public BotFrameworkConfigurationBuilder UseCredentialProvider(ICredentialProvider credentialProvider)
        {
            _options.CredentialProvider = credentialProvider;

            return this;
        }

        public BotFrameworkConfigurationBuilder UseMiddleware(IMiddleware middleware)
        {
            _options.Middleware.Add(middleware);

            return this;
        }

        /// <summary>
        /// Adds retry policy on failure for BotFramework calls.
        /// </summary>
        /// <param name="retryPolicy">The retry policy.</param>
        /// <returns><see cref="BotFrameworkConfigurationBuilder"/> instance with the retry policy set.</returns>
        public BotFrameworkConfigurationBuilder UseRetryPolicy(RetryPolicy retryPolicy)
        {
            _options.ConnectorClientRetryPolicy = retryPolicy;
            return this;
        }

        public BotFrameworkConfigurationBuilder EnableProactiveMessages(string proactiveMessagesPath = default(string))
        {
            _options.EnableProactiveMessages = true;
            
            if (proactiveMessagesPath != null)
            
{
                _options.Paths.ProactiveMessagesPath = proactiveMessagesPath;
            }

            return this;
        }

        public BotFrameworkConfigurationBuilder UsePaths(Action<BotFrameworkPaths> configurePaths)
        {
            configurePaths(_options.Paths);

            return this;
        }
    }
}