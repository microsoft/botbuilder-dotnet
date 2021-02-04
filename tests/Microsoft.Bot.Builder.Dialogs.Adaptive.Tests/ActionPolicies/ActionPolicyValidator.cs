// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    /// <summary>
    /// Validator used to verify a dialog with its triggers and actions are not violating
    /// any Action Policies. ValidatePolicies will throw an <see cref="ActionPolicyException"/>
    /// if any policy violations are found.
    /// </summary>
    internal class ActionPolicyValidator
    {
        private readonly ResourceExplorer _resources;
        
        public ActionPolicyValidator(ResourceExplorer resources)
        {
            _resources = resources;
        }

        public void ValidatePolicies(Dialog dialog)
        {
            // validate policies for all triggers and their child actions
            foreach (var trigger in (dialog as AdaptiveDialog)?.Triggers)
            {
                ValidateCondition(trigger);
            }
        }

        private void ValidateCondition(OnCondition condition)
        {
            var triggerKinds = _resources.GetKindsForType(condition.GetType());
            foreach (var triggerKind in triggerKinds)
            {
                List<string> parentKinds = new List<string>();
                parentKinds.Add(triggerKind);

                // Validate the actions of the trigger
                ValidateKind(parentKinds, triggerKind, condition.Actions);
            }
        }

        private void ValidateKind(List<string> parentKinds, string kind, List<Dialog> dialogs)
        {
            var kindPolicy = ActionPolicies.FirstOrDefault(p => p.Kind == kind);
            if (kindPolicy != null)
            {
                ValidatePolicy(parentKinds, kindPolicy, dialogs);
            }

            if (dialogs != null)
            {
                foreach (var dialog in dialogs)
                {
                    var actionKinds = _resources.GetKindsForType(dialog.GetType());
                    foreach (var actionKind in actionKinds)
                    {
                        List<string> parentKindsInner = new List<string>(parentKinds);
                        parentKindsInner.Add(actionKind);

                        var actionPolicy = ActionPolicies.FirstOrDefault(p => p.Kind == actionKind);
                        if (actionPolicy != null)
                        {
                            ValidatePolicy(parentKindsInner, actionPolicy, dialogs, dialog);
                        }

                        ValidateKind(parentKindsInner, actionKind, GetDialogs(dialog));
                    }
                }
            }
        }

        private void ValidatePolicy(List<string> parentKinds, ActionPolicy policy, List<Dialog> dialogs, Dialog dialog = null)
        {
            switch (policy.Type)
            {
                case ActionPolicyType.LastAction:
                    // This dialog must be the last in the list
                    if (dialogs.IndexOf(dialog) < dialogs.Count - 1)
                    {
                        throw new ActionPolicyException(policy, dialog);
                    }

                    break;
                case ActionPolicyType.Interactive:
                    // This dialog is interactive, so cannot be under NonInteractive triggers
                    foreach (var parentKind in parentKinds)
                    {
                        var parentPolicy = ActionPolicies.FirstOrDefault(p => p.Kind == parentKind);
                        if (parentPolicy != null && parentPolicy.Type == ActionPolicyType.TriggerNotInteractive)
                        {
                            throw new ActionPolicyException(policy, dialog);
                        }
                    }

                    break;
                case ActionPolicyType.AllowedTrigger:
                    // ensure somewhere up the chain the specific trigger type is found
                    if (parentKinds.Any(pk => policy.Actions.Contains(pk)))
                    {
                        return;
                    }
                    
                    // Trigger type not found up the chain.  This action is in the wrong trigger.
                    throw new ActionPolicyException(policy, dialog);

                case ActionPolicyType.TriggerNotInteractive:
                    // ensure no dialogs, or child dialogs, are Input dialogs
                    var childDialogs = dialogs.Where(d => d.Id != dialog?.Id).ToList();
                    while (childDialogs.Count > 0)
                    {
                        var childDialog = childDialogs[0];
                        var childKinds = _resources.GetKindsForType(childDialog.GetType());
                        foreach (var childKind in childKinds)
                        {
                            var childPolicy = ActionPolicies.FirstOrDefault(p => p.Kind == childKind);
                            if (childPolicy != null && childPolicy.Type == ActionPolicyType.Interactive)
                            {
                                // Interactive action found below TriggerNotInteractive trigger
                                throw new ActionPolicyException(policy, dialog);
                            }
                        }

                        childDialogs.RemoveAt(0);

                        var innerChildDialogs = GetDialogs(childDialog);
                        if (innerChildDialogs != null) 
                        {
                            childDialogs.AddRange(innerChildDialogs);
                        }
                    }

                    break;
                case ActionPolicyType.TriggerRequiresAction:
                    // ensure the required action is present in the dialogs chain
                    var childActions = dialogs.Where(d => d.Id != dialog?.Id).ToList();
                    while (childActions.Count > 0)
                    {
                        var childDialog = childActions[0];
                        var childKinds = _resources.GetKindsForType(childDialog.GetType());
                        if (policy.Actions.Any(policyAction => childKinds.Contains(policyAction)))
                        {
                            // found the action required
                            return;
                        }

                        childActions.RemoveAt(0);

                        var innerChildDialogs = GetDialogs(childDialog);
                        if (innerChildDialogs != null)
                        {
                            childActions.AddRange(innerChildDialogs);
                        }
                    }

                    // Required action not found
                    throw new ActionPolicyException(policy, dialog);

                default:
                    throw new InvalidOperationException($"Invalid ActionPolicy.ActionPolicyType: {policy.Type}");
            }
        }

        private List<Dialog> GetDialogs(Dialog dialog)
        {
            if (dialog is AdaptiveDialog asAdaptive)
            {
                return asAdaptive.Dialogs.GetDialogs().ToList();
            }

            if (dialog is ActionScope asActionScope)
            {
                return asActionScope.Actions.ToList();
            }

            if (dialog is IDialogDependencies dependencies)
            {
                var dialogs = new List<Dialog>();
                foreach (var childDialog in dependencies.GetDependencies())
                {
                    if (childDialog is ActionScope scope)
                    {
                        foreach (var action in scope.Actions)
                        {
                            dialogs.Add(action);
                        }
                    }
                    else
                    {
                        dialogs.Add(childDialog);
                    }
                }

                return dialogs;
            }

            return null;
        }

#pragma warning disable SA1201 // Elements should appear in the correct order
        private static IEnumerable<ActionPolicy> ActionPolicies
#pragma warning restore SA1201 // Elements should appear in the correct order
        {
            get
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
}
