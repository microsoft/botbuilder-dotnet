// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma warning disable SA1201 // Elements should appear in the correct order

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.Luis.Testing;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class TestScriptTests
    {
        private readonly string luisMockDirectory = PathUtils.NormalizePath(@"..\..\..\..\..\tests\Microsoft.Bot.Builder.Dialogs.Adaptive.Tests\Tests\TestScriptTests\LuisMock\");

        public static ResourceExplorer ResourceExplorer { get; set; }

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            ResourceExplorer = new ResourceExplorer()
                .AddFolder(Path.Combine(TestUtils.GetProjectPath(), "Tests", nameof(TestScriptTests)), monitorChanges: false);
        }

#if AUTO
        public static IEnumerable<object[]> AssertReplyScripts => TestUtils.GetTestScripts(@"Tests\TestAssertReply");

        [DataTestMethod]
        [DynamicData(nameof(AssertReplyScripts))]
        public async Task TestScript_AssertReply(string resourceId)
        {
            await TestUtils.RunTestScript(resourceId);
        }
#endif
        [TestMethod]
        public async Task TestScriptTests_AssertReplyOneOf()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task TestScriptTests_AssertReplyOneOf_Assertions()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task TestScriptTests_AssertReplyOneOf_exact()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task TestScriptTests_AssertReplyOneOf_User()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task TestScriptTests_AssertReply_Assertions()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task TestScriptTests_AssertReply_Assertions_Failed()
        {
            try
            {
                await TestUtils.RunTestScript(ResourceExplorer);
            }
            catch (Exception e)
            {
                Assert.IsTrue(e.Message.Contains("\"text\": \"hi User1\""));
            }
        }

        [TestMethod]
        public async Task TestScriptTests_AssertReply_Exact()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task TestScriptTests_AssertReply_User()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task TestScriptTests_HttpRequestMock()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task TestScriptTests_HttpRequestLuisMock()
        {
            var config = new ConfigurationBuilder()
                .UseMockLuisSettings(luisMockDirectory, "TestBot")
                .Build();

            var resourceExplorer = new ResourceExplorer()
                .AddFolder(Path.Combine(TestUtils.GetProjectPath(), "Tests", nameof(TestScriptTests)), monitorChanges: false)
                .RegisterType(LuisAdaptiveRecognizer.Kind, typeof(MockLuisRecognizer), new MockLuisLoader(config));

            await TestUtils.RunTestScript(resourceExplorer, configuration: config);
        }

        [TestMethod]
        public async Task TestScriptTests_CustomEvent()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task TestScriptTests_PropertyMock()
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddInMemoryCollection(new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("file", "set settings.file"),
                new KeyValuePair<string, string>("fileoverwrite", "this is overwritten")
            });

            await TestUtils.RunTestScript(ResourceExplorer, configuration: configBuilder.Build());
        }

        [TestMethod]
        public async Task TestScriptTests_UserConversationUpdate()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task TestScriptTests_UserTokenMock()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task TestScriptTests_UserTyping()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }
    }
}
