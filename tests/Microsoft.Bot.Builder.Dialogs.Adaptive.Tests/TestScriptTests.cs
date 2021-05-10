// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma warning disable SA1201 // Elements should appear in the correct order

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.Luis.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [CollectionDefinition("Dialogs.Adaptive")]
    public class TestScriptTests : IClassFixture<ResourceExplorerFixture>
    {
        private readonly string luisMockDirectory = PathUtils.NormalizePath(@"..\..\..\..\..\tests\Microsoft.Bot.Builder.Dialogs.Adaptive.Tests\Tests\TestScriptTests\LuisMock\");
        private readonly ResourceExplorerFixture _resourceExplorerFixture;

        public TestScriptTests(ResourceExplorerFixture resourceExplorerFixture)
        {
            _resourceExplorerFixture = resourceExplorerFixture.Initialize(nameof(TestScriptTests));
        }

#if AUTO
        public static IEnumerable<object[]> AssertReplyScripts => TestUtils.GetTestScripts(@"Tests\TestAssertReply");

        [Theory]
        [MemberData(nameof(AssertReplyScripts), DisableDiscoveryEnumeration = true)]
        public async Task TestScript_AssertReply(string resourceId)
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, resourceId: resourceId);
        }
#endif

        [Fact]
        public async Task TestScriptTests_AssertReplyOneOf()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task TestScriptTests_AssertReplyOneOf_Assertions()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task TestScriptTests_AssertReplyOneOf_exact()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task TestScriptTests_AssertReplyOneOf_User()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task TestScriptTests_AssertReply_Assertions()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task TestScriptTests_AssertReply_Assertions_Failed()
        {
            try
            {
                await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
            }
            catch (Exception e)
            {
                Assert.Contains("\"text\": \"hi User1\"", e.Message);
            }
        }

        [Fact]
        public async Task TestScriptTests_AssertReply_Exact()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task TestScriptTests_AssertReply_User()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task TestScriptTests_HttpRequestMock()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task TestScriptTests_HttpRequestLuisMock()
        {
            var config = new ConfigurationBuilder()
                .UseMockLuisSettings(luisMockDirectory, "TestBot")
                .Build();

            var resourceExplorer = _resourceExplorerFixture.ResourceExplorer
                .RegisterType(LuisAdaptiveRecognizer.Kind, typeof(MockLuisRecognizer), new MockLuisLoader(config));

            await TestUtils.RunTestScript(resourceExplorer, configuration: config);
        }

        [Fact]
        public async Task TestScriptTests_HttpRequestQnAMakerRecognizerMock()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task TestScriptTests_HttpRequestQnAMakerDialogMock()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task TestScriptTests_CustomEvent()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task TestScriptTests_PropertyMock()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task TestScriptTests_SettingMock()
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddInMemoryCollection(new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("file", "set settings.file"),
                new KeyValuePair<string, string>("fileoverwrite", "this is overwritten")
            });

            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, configuration: configBuilder.Build());
        }

        [Fact]
        public async Task TestScriptTests_UserActivity()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task TestScriptTests_UserConversationUpdate()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task TestScriptTests_OAuthInputMockProperties()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task TestScriptTests_OAuthInputLG()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task TestScriptTests_UserTokenMock()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task TestScriptTests_UserTyping()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task TestScriptTests_OAuthInputRetries_WithNullMessageText()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }
    }
}
