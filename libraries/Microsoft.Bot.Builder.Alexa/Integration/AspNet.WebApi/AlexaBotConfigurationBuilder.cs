// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder.Alexa.Integration.AspNet.WebApi
{
    public class AlexaBotConfigurationBuilder
    {
        private readonly AlexaBotOptions _options;

        public AlexaBotConfigurationBuilder(AlexaBotOptions alexaBotOptions)
        {
            _options = alexaBotOptions;
        }

        public AlexaBotOptions AlexaBotOptions => _options;

        public AlexaBotConfigurationBuilder UseMiddleware(IMiddleware middleware)
        {
            _options.Middleware.Add(middleware);
            return this;
        }

        public AlexaBotConfigurationBuilder UsePaths(Action<AlexaBotPaths> configurePaths)
        {
            configurePaths(_options.Paths);
            return this;
        }
    }
}