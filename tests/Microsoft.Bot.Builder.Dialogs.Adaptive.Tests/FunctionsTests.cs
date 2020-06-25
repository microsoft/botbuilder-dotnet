// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AdaptiveExpressions;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Functions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Tests;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Functions;
using Microsoft.Bot.Builder.Dialogs.Memory;
using Microsoft.Bot.Builder.Dialogs.Memory.Scopes;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class FunctionsTests
    {
        public static ResourceExplorer ResourceExplorer { get; set; }

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            ResourceExplorer = new ResourceExplorer()
                .AddFolder(Path.Combine(TestUtils.GetProjectPath(), "Tests", nameof(FunctionsTests)), monitorChanges: false);

            // this will test that we are registering the custom functions
            var component = new AdaptiveComponentRegistration();
        }

        [TestMethod]
        public void IsDialogActive_Variations()
        {
            var config = new DialogStateManagerConfiguration()
            {
                MemoryScopes = new List<MemoryScope>()
                {
                    new MockMemoryScope("dialogContext", new { stack = new[] { "a", "d", "F" } })
                }
            };
            var dc = new DialogContext(new DialogSet(), new TurnContext(new TestAdapter(), new Schema.Activity()), new DialogState());
            DialogStateManager dsm = new DialogStateManager(dc, config);

            Assert.AreEqual(true, Expression.Parse("isDialogActive('a')").TryEvaluate(dsm).value);
            Assert.AreEqual(true, Expression.Parse("isDialogActive('b','c','d')").TryEvaluate(dsm).value);
            Assert.AreEqual(false, Expression.Parse("isDialogActive('b','c','e')").TryEvaluate(dsm).value);
            Assert.AreEqual(false, Expression.Parse("isDialogActive('c')").TryEvaluate(dsm).value);
            Assert.AreEqual(false, Expression.Parse("isDialogActive('f')").TryEvaluate(dsm).value);
            Assert.AreEqual(true, Expression.Parse("isDialogActive('F')").TryEvaluate(dsm).value);
        }

        [TestMethod]
        public async Task IsDialogActive()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task HasPendingActions()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        public class MockMemoryScope : MemoryScope
        {
            private object memory;

            public MockMemoryScope(string name, object memory)
                : base(name, false)
            {
                this.memory = JObject.FromObject(memory);
            }

            public override object GetMemory(DialogContext dc)
            {
                return this.memory;
            }

            public override void SetMemory(DialogContext dc, object memory)
            {
                throw new NotSupportedException("You can't modify the dialogcontext scope");
            }
        }
    }
}
