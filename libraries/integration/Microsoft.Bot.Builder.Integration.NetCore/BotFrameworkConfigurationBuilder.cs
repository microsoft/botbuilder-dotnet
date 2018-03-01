using Microsoft.Bot.Builder.Middleware;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Integration.NetCore
{
    internal sealed class BotFrameworkConfigurationBuilder : IBotFraweworkConfigurationBuilder
    {
        private readonly BotFrameworkOptions _options;

        public BotFrameworkConfigurationBuilder(BotFrameworkOptions options)
        {
            _options = options;
        }

        public IBotFraweworkConfigurationBuilder UseApplicationIdentity(string applicationId, string applicationPassword)
        {
            _options.ApplicationId = applicationId;
            _options.ApplicationPassword = applicationPassword;

            return this;
        }

        public IBotFraweworkConfigurationBuilder UseMiddleware(IMiddleware middleware)
        {
            _options.Middleware.Add(middleware);

            return this;
        }
    }
}
