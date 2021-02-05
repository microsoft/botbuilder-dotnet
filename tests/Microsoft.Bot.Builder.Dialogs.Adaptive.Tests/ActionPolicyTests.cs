// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma warning disable SA1118 // Parameter should not span multiple lines

using System.Runtime.CompilerServices;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [CollectionDefinition("Dialogs.Adaptive")]
    public class ActionPolicyTests : IClassFixture<ResourceExplorerFixture>
    {
        private readonly ResourceExplorerFixture _resourceExplorerFixture;
        private readonly ActionPolicyValidator _validator;

        public ActionPolicyTests(ResourceExplorerFixture resourceExplorerFixture)
        {
            _resourceExplorerFixture = resourceExplorerFixture.Initialize(nameof(ActionPolicyTests));
            _validator = new ActionPolicyValidator(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public void LastAction_BreakLoop_Invalid()
        {
            var ex = Assert.Throws<ActionPolicyException>(() => RunActionPolicyValidator());

            Assert.Equal(ActionPolicyType.LastAction, ex.ActionPolicy.Type);
            Assert.Equal(BreakLoop.Kind, ex.ActionPolicy.Kind);
        }

        [Fact]
        public void LastAction_CancelAllDialogs_Invalid()
        {
            var ex = Assert.Throws<ActionPolicyException>(() => RunActionPolicyValidator());

            Assert.Equal(ActionPolicyType.LastAction, ex.ActionPolicy.Type);
            Assert.Equal(CancelAllDialogs.Kind, ex.ActionPolicy.Kind);
        }

        [Fact]
        public void TriggerNotInteractive_OnEndOfConversationActivity_Invalid()
        {
            var ex = Assert.Throws<ActionPolicyException>(() => RunActionPolicyValidator());

            Assert.Equal(ActionPolicyType.TriggerNotInteractive, ex.ActionPolicy.Type);
            Assert.Equal(OnEndOfConversationActivity.Kind, ex.ActionPolicy.Kind);
        }

        [Fact]
        public void OnEndOfConversationActivity_Valid()
        {
            // No exception for valid trigger validation
            RunActionPolicyValidator();
        }

        private void RunActionPolicyValidator([CallerMemberName] string testName = null)
        {
            var script = _resourceExplorerFixture.ResourceExplorer.LoadType<TestScript>($"{testName}.test.dialog");
            script.Configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();

            _validator.ValidatePolicies(script.Dialog);
        }
    }
}
