// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Integration.Runtime;
using Microsoft.Bot.Builder.Integration.Runtime.Extensions;
using Microsoft.Bot.Builder.Integration.Runtime.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests.Extensions
{
    public class AdapterRegistrationTests
    {
        [Fact]
        public void CoreBotAdapterRegistered()
        {
            // Setup
            IServiceCollection services = new ServiceCollection();
            IConfiguration configuration = new ConfigurationBuilder().Build();

            // We do this here since in asp.net core world this is done for us, but here we need to do it manually.
            services.AddSingleton(configuration);

            // Test
            services.AddBotRuntime(configuration);

            // Assert
            var provider = services.BuildServiceProvider();

            // Core adapter should be register for as IBotFrameworkHttpAdapter for controllers
            Assertions.AssertService<IBotFrameworkHttpAdapter, CoreBotAdapter>(services, provider, ServiceLifetime.Singleton);

            // Core adapter should be register for as BotAdapter for Skill HttpClient
            Assertions.AssertService<BotAdapter, CoreBotAdapter>(services, provider, ServiceLifetime.Singleton);
        }
    }
}
