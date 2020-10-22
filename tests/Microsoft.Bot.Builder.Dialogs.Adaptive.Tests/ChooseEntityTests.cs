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
    public class ChooseEntityTests : IClassFixture<ResourceExplorerFixture>
    {
        private readonly string chooseEntityDirectory = PathUtils.NormalizePath(@"..\..\..\..\..\tests\Microsoft.Bot.Builder.Dialogs.Adaptive.Tests\Tests\chooseEntityTests\");
        private readonly IConfiguration _configuration;
        private readonly ResourceExplorerFixture _resourceExplorerFixture;

        public ChooseEntityTests(ResourceExplorerFixture resourceExplorerFixture)
        {
            _resourceExplorerFixture = resourceExplorerFixture.Initialize(nameof(ChooseEntityTests));

            _configuration = new ConfigurationBuilder()
                .UseMockLuisSettings(chooseEntityDirectory, "TestBot")
                .Build();

            _resourceExplorerFixture.ResourceExplorer
                .RegisterType(LuisAdaptiveRecognizer.Kind, typeof(MockLuisRecognizer), new MockLuisLoader(_configuration));
        }

        [Fact]
        public async Task ChooseEntity()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, configuration: _configuration);
        }
    }
}
