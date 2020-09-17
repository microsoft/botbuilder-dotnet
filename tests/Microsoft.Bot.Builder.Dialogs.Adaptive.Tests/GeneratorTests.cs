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
    public class GeneratorTests : IClassFixture<ResourceExplorerFixture>
    {
        private readonly string sandwichDirectory = PathUtils.NormalizePath(@"..\..\..\..\..\tests\Microsoft.Bot.Builder.Dialogs.Adaptive.Tests\Tests\GeneratorTests\sandwich\");
        private readonly IConfiguration _configuration;
        private readonly ResourceExplorerFixture _resourceExplorerFixture;

        public GeneratorTests(ResourceExplorerFixture resourceExplorerFixture)
        {
            _resourceExplorerFixture = resourceExplorerFixture.Initialize(nameof(GeneratorTests));

            _configuration = new ConfigurationBuilder()
                .UseMockLuisSettings(sandwichDirectory, "TestBot")
                .Build();

            _resourceExplorerFixture.ResourceExplorer
                .RegisterType(LuisAdaptiveRecognizer.Kind, typeof(MockLuisRecognizer), new MockLuisLoader(_configuration));
        }

        [Fact]
        public async Task Generator_sandwich()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, configuration: _configuration);
        }
    }
}
