// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Runtime.Tests.Components
{
    public class TestBotComponent : BotComponent
    {
        private readonly Action<IServiceCollection, IConfiguration, ILogger> _loadAction;

        public TestBotComponent(Action<IServiceCollection, IConfiguration, ILogger> loadAction)
        {
            _loadAction = loadAction ?? throw new ArgumentNullException(nameof(loadAction));
        }

        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration, ILogger logger)
        {
            _loadAction(services, configuration, logger);
        }
    }
}
