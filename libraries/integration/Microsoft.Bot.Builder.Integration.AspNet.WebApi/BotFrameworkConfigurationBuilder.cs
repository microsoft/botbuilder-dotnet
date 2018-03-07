// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Connector.Authentication;

namespace Microsoft.Bot.Builder.Integration.AspNet.WebApi
{
    public class BotFrameworkConfigurationBuilder
    {
        private BotFrameworkOptions _options;

        public BotFrameworkConfigurationBuilder()
        {
            _options = new BotFrameworkOptions();
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
    }
}