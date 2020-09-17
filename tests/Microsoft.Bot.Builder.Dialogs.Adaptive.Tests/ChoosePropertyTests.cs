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
    public class ChoosePropertyTests : IClassFixture<ResourceExplorerFixture>
    {
        private readonly string choosePropertyDirectory = PathUtils.NormalizePath(@"..\..\..\..\..\tests\Microsoft.Bot.Builder.Dialogs.Adaptive.Tests\Tests\choosePropertyTests\");
        private readonly IConfiguration _configuration;
        private readonly ResourceExplorerFixture _resourceExplorerFixture;

        public ChoosePropertyTests(ResourceExplorerFixture resourceExplorerFixture)
        {
            _resourceExplorerFixture = resourceExplorerFixture.Initialize(nameof(ChoosePropertyTests));

            _configuration = new ConfigurationBuilder()
                .UseMockLuisSettings(choosePropertyDirectory, "TestBot")
                .Build();

            _resourceExplorerFixture.ResourceExplorer
                .RegisterType(LuisAdaptiveRecognizer.Kind, typeof(MockLuisRecognizer), new MockLuisLoader(_configuration));
        }

        [Fact]
        public async Task ChooseProperty()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, configuration: _configuration);
        }
    }
}
