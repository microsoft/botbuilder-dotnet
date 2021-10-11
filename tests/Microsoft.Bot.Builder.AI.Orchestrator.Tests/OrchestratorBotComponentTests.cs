// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Bot.Builder.AI.Orchestrator.Tests
{
    public class OrchestratorBotComponentTests
    {
        [Fact]
        public void TestConfigureServices()
        {
            var service = new ServiceCollection();
            new OrchestratorBotComponent().ConfigureServices(service, new ConfigurationBuilder().Build());
            Assert.Equal(ServiceLifetime.Singleton, service[0].Lifetime);
        }
    }
}
