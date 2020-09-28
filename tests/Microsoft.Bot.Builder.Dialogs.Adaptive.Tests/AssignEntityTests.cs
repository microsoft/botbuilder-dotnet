// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.Luis.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [CollectionDefinition("Dialogs.Adaptive")]
    public class AssignEntityTests : IClassFixture<ResourceExplorerFixture>
    {
        private readonly string assignEntityDirectory = PathUtils.NormalizePath(@"..\..\..\..\..\tests\Microsoft.Bot.Builder.Dialogs.Adaptive.Tests\Tests\AssignEntityTests\");
        private readonly IConfiguration _configuration;
        private readonly ResourceExplorerFixture _resourceExplorerFixture;

        public AssignEntityTests(ResourceExplorerFixture resourceExplorerFixture)
        {
            _resourceExplorerFixture = resourceExplorerFixture.Initialize(nameof(AssignEntityTests));

            _configuration = new ConfigurationBuilder()
                .UseMockLuisSettings(assignEntityDirectory, "TestBot")
                .Build();

            _resourceExplorerFixture.ResourceExplorer
                .RegisterType(LuisAdaptiveRecognizer.Kind, typeof(MockLuisRecognizer), new MockLuisLoader(_configuration));
        }

        [Fact]
        public async Task AddEntity()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, configuration: _configuration);
        }

        [Fact]
        public async Task ClearEntity()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, configuration: _configuration);
        }

        [Fact]
        public async Task ShowEntity()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, configuration: _configuration);
        }

        [Fact]
        public async Task HelpEntity()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, configuration: _configuration);
        }
    }
}
