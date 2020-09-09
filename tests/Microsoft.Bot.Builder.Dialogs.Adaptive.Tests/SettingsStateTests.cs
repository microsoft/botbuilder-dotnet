// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [CollectionDefinition("Dialogs.Adaptive")]
    public class SettingsStateTests : IClassFixture<ResourceExplorerFixture>, IClassFixture<ConfigurationFixture>
    {
        private readonly ResourceExplorerFixture _resourceExplorerFixture;
        private readonly ConfigurationFixture _configurationFixture;

        public SettingsStateTests(ResourceExplorerFixture resourceExplorerFixture, ConfigurationFixture configurationFixture)
        {
            _resourceExplorerFixture = resourceExplorerFixture.Initialize(nameof(SettingsStateTests));
            _configurationFixture = configurationFixture;
        }

        [Fact]
        public async Task SettingsStateTests_SettingsTest()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, configuration: _configurationFixture.Configuration);
        }

        [Fact]
        public async Task SettingsStateTests_TestTurnStateAcrossBoundaries()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, configuration: _configurationFixture.Configuration);
        }
    }
}
