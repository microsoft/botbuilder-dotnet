// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [CollectionDefinition("Dialogs.Adaptive")]
    public class SelectorTests : IClassFixture<ResourceExplorerFixture>
    {
        private readonly ResourceExplorerFixture _resourceExplorerFixture;

        public SelectorTests(ResourceExplorerFixture resourceExplorerFixture)
        {
            _resourceExplorerFixture = resourceExplorerFixture.Initialize(nameof(SelectorTests));
        }

        [Fact]
        public async Task SelectorTests_FirstSelector()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task SelectorTests_RandomSelector()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task SelectorTests_MostSpecificFirstSelector()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task SelectorTests_MostSpecificRandomSelector()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task SelectorTests_AdaptiveTrueSelector()
        {
            // only execute first selection
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, resourceId: "SelectorTests_TrueSelector.test.dialog");
        }

        [Fact]
        public async Task SelectorTests_AdaptiveConditionalSelector()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, resourceId: "SelectorTests_ConditionalSelector.test.dialog");
        }

        [Fact]
        public async Task SelectorTests_RunOnce()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task SelectorTests_Priority()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task SelectorTests_Float_Priority()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }
    }
}
