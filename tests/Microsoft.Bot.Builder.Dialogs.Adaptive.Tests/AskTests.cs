// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma warning disable SA1118 // Parameter should not span multiple lines
#pragma warning disable SA1210 // namespace order

using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.Luis.Testing;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class AskTests
    {
        public static ResourceExplorer ResourceExplorer { get; set; }

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            var path = Path.Combine(TestUtils.GetProjectPath(), "Tests", nameof(AskTests));
            ResourceExplorer = new ResourceExplorer()
                .AddFolder(path, monitorChanges: false)
                .RegisterType(LuisAdaptiveRecognizer.Kind, typeof(MockLuisRecognizer), new MockLuisLoader());
        }

        [TestMethod]
        public async Task AskRetriesSetProperty()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task AskRetriesSetProperties()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task AskRetriesDeleteProperty()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task AskRetriesDeleteProperties()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task AskRetriesEditArray()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }
    }
}
