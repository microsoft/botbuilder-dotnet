// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma warning disable SA1118 // Parameter should not span multiple lines

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.ActionPolicies;
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
            _validator = new ActionPolicyValidator(_resourceExplorerFixture.ResourceExplorer, GetActionPolicies());
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

#pragma warning disable SA1204 // Elements should appear in the correct order
        private static IEnumerable<ActionPolicy> GetActionPolicies()
#pragma warning restore SA1204 // Elements should appear in the correct order
        {
            // LastAction (dialog continues)
            yield return new ActionPolicy(BreakLoop.Kind, ActionPolicyType.LastAction);
            yield return new ActionPolicy(ContinueLoop.Kind, ActionPolicyType.LastAction);
            yield return new ActionPolicy(GotoAction.Kind, ActionPolicyType.LastAction);

            // LastAction (dialog ends)
            yield return new ActionPolicy(CancelDialog.Kind, ActionPolicyType.LastAction);
            yield return new ActionPolicy(CancelAllDialogs.Kind, ActionPolicyType.LastAction);
            yield return new ActionPolicy(EndDialog.Kind, ActionPolicyType.LastAction);
            yield return new ActionPolicy(RepeatDialog.Kind, ActionPolicyType.LastAction);
            yield return new ActionPolicy(ReplaceDialog.Kind, ActionPolicyType.LastAction);
            yield return new ActionPolicy(ThrowException.Kind, ActionPolicyType.LastAction);

            // Interactive (Input Dialogs)
            yield return new ActionPolicy(Ask.Kind, ActionPolicyType.Interactive);
            yield return new ActionPolicy(AttachmentInput.Kind, ActionPolicyType.Interactive);
            yield return new ActionPolicy(ChoiceInput.Kind, ActionPolicyType.Interactive);
            yield return new ActionPolicy(ConfirmInput.Kind, ActionPolicyType.Interactive);
            yield return new ActionPolicy(DateTimeInput.Kind, ActionPolicyType.Interactive);
            yield return new ActionPolicy(NumberInput.Kind, ActionPolicyType.Interactive);
            yield return new ActionPolicy(OAuthInput.Kind, ActionPolicyType.Interactive);
            yield return new ActionPolicy(TextInput.Kind, ActionPolicyType.Interactive);

            // TriggerNotInteractive (no intput dialogs)
            yield return new ActionPolicy(OnEndOfConversationActivity.Kind, ActionPolicyType.TriggerNotInteractive);
        }
    }
}
