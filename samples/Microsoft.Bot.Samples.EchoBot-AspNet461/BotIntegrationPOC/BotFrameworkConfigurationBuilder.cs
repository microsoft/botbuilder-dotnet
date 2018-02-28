// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Microsoft.Bot.Builder.Middleware;

namespace Microsoft.Bot.Samples.EchoBot_AspNet461.Integration
{
    public class BotFrameworkConfigurationBuilder
    {
        private BotFrameworkOptions _options;

        public BotFrameworkConfigurationBuilder()
        {
            _options = new BotFrameworkOptions();
        }

        public BotFrameworkOptions BotFrameworkOptions { get => _options; }

        public BotFrameworkConfigurationBuilder UseApplicationIdentity(string applicationId, string applicationPassword)
        {
            _options.AppId = applicationId;
            _options.AppPassword = applicationPassword;

            return this;
        }

        public BotFrameworkConfigurationBuilder UseMiddleware(IMiddleware middleware)
        {
            _options.Middleware.Add(middleware);

            return this;
        }
    }
}