// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.ActionPolicies
{
    /// <summary>
    /// Validator used to verify a dialog with its triggers and actions are not violating
    /// any Action Policies. ValidatePolicies will throw an <see cref="ActionPolicyException"/>
    /// if any policy violations are found.
    /// </summary>
    public class ActionPolicyValidator
    {
        private readonly ResourceExplorer _resources;
        private readonly IEnumerable<ActionPolicy> _actionPolicies;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionPolicyValidator"/> class.
        /// </summary>
        /// <param name="resources">The resources contining the kind and types used for validation.</param>
        /// <param name="actionPolicies">Policies to use during validation.</param>
        public ActionPolicyValidator(ResourceExplorer resources, IEnumerable<ActionPolicy> actionPolicies)
        {
            _resources = resources;
            _actionPolicies = actionPolicies;
        }

        /// <summary>
        /// Validates an Adaptive Dialog's Triggers against action policies.
        /// </summary>
        /// <param name="dialog">The dialog to validate.</param>
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
            var kindPolicy = _actionPolicies.FirstOrDefault(p => p.Kind == kind);
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

                        var actionPolicy = _actionPolicies.FirstOrDefault(p => p.Kind == actionKind);
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
                        var parentPolicy = _actionPolicies.FirstOrDefault(p => p.Kind == parentKind);
                        if (parentPolicy != null && parentPolicy.Type == ActionPolicyType.TriggerNotInteractive)
                        {
                            throw new ActionPolicyException(policy, dialog);
                        }
                    }

                    break;
                case ActionPolicyType.AllowedTrigger:
                    // ensure somewhere up the chain the specific trigger type is found
                    if (parentKinds.Any(pk => policy.Kinds.Contains(pk)))
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
                            var childPolicy = _actionPolicies.FirstOrDefault(p => p.Kind == childKind);
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
                        if (policy.Kinds.Any(policyAction => childKinds.Contains(policyAction)))
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
    }
}
