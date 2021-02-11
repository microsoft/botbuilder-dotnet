// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdaptiveExpressions;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Memory;
using Microsoft.Bot.Builder.Dialogs.Memory.Scopes;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [CollectionDefinition("Dialogs.Adaptive")]
    public class FunctionsTests : IClassFixture<ResourceExplorerFixture>
    {
        private readonly ResourceExplorerFixture _resourceExplorerFixture;

        public FunctionsTests(ResourceExplorerFixture resourceExplorerFixture)
        {
            _resourceExplorerFixture = resourceExplorerFixture.Initialize(nameof(FunctionsTests));

            // this will test that we are registering the custom functions
            new AdaptiveComponentRegistration();
        }

        [Fact]
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
            var dsm = new DialogStateManager(dc, config);

            Assert.True((bool)Expression.Parse("isDialogActive('a')").TryEvaluate(dsm).value);
            Assert.True((bool)Expression.Parse("isDialogActive('b','c','d')").TryEvaluate(dsm).value);
            Assert.False((bool)Expression.Parse("isDialogActive('b','c','e')").TryEvaluate(dsm).value);
            Assert.False((bool)Expression.Parse("isDialogActive('c')").TryEvaluate(dsm).value);
            Assert.False((bool)Expression.Parse("isDialogActive('f')").TryEvaluate(dsm).value);
            Assert.True((bool)Expression.Parse("isDialogActive('F')").TryEvaluate(dsm).value);
        }

        [Fact]
        public async Task IsDialogActive()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task HasPendingActions()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        public class MockMemoryScope : MemoryScope
        {
            private readonly object memory;

            public MockMemoryScope(string name, object memory)
                : base(name, false)
            {
                this.memory = JObject.FromObject(memory);
            }

            public override object GetMemory(DialogContext dc)
            {
                return memory;
            }

            public override void SetMemory(DialogContext dc, object memory)
            {
                throw new NotSupportedException("You can't modify the dialogcontext scope");
            }
        }
    }
}
