// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Tests;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers.Tests
{
    [CollectionDefinition("Dialogs.Adaptive.Recognizers")]
    public class MultiLanguageRecognizerTests : IClassFixture<ResourceExplorerFixture>
    {
        private readonly ResourceExplorerFixture _resourceExplorerFixture;

        public MultiLanguageRecognizerTests(ResourceExplorerFixture resourceExplorerFixture)
        {
            _resourceExplorerFixture = resourceExplorerFixture.Initialize(nameof(MultiLanguageRecognizerTests));
        }

        [Fact]
        public async Task MultiLanguageRecognizerTest_EnUsFallback()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task MultiLanguageRecognizerTest_EnUsFallback_ActivityLocaleCasing()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task MultiLanguageRecognizerTest_EnGbFallback()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task MultiLanguageRecognizerTest_EnFallback()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task MultiLanguageRecognizerTest_DefaultFallback()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task MultiLanguageRecognizerTest_LanguagePolicy()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }
    }
}
