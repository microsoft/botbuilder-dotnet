// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [CollectionDefinition("Dialogs.Adaptive")]
    public class SettingsStateTests : IClassFixture<ResourceExplorerFixture>
    {
        private readonly ResourceExplorerFixture _resourceExplorerFixture;

        public SettingsStateTests(ResourceExplorerFixture resourceExplorerFixture)
        {
            _resourceExplorerFixture = resourceExplorerFixture.Initialize(nameof(SettingsStateTests));
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .Build();
        }

        public static IConfiguration Configuration { get; set; }

        [Fact]
        public async Task SettingsStateTests_SettingsTest()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, configuration: Configuration);
        }

        [Fact]
        public async Task SettingsStateTests_TestTurnStateAcrossBoundaries()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, configuration: Configuration);
        }
    }
}
