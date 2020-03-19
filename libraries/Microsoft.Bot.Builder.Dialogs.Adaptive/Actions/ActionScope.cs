// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// ActionScope manages execution of a block of actions, and supports Goto, Continue and Break semantics..
    /// </summary>
    public class ActionScope : Dialog, IDialogDependencies
    {
        protected const string OFFSETKEY = "this.offset";

        public ActionScope(IEnumerable<Dialog> actions = null)
        {
            if (actions != null)
            {
                this.Actions = new List<Dialog>(actions);
            }
        }

        /// <summary>
        /// Gets or sets the actions to execute.
        /// </summary>
        /// <value>The actions to execute.</value>
        public List<Dialog> Actions { get; set; } = new List<Dialog>();

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            if (this.Actions.Any())
            {
                return await this.BeginActionAsync(dc, 0, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                return await dc.EndDialogAsync(null, cancellationToken).ConfigureAwait(false);
            }
        }

        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            // We're being continued after an interruption so just run next action
            return await OnNextActionAsync(dc, null, cancellationToken).ConfigureAwait(false);
        }

        public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default)
        {
            if (result is ActionScopeResult actionScopeResult)
            {
                return await OnActionScopeResultAsync(dc, actionScopeResult, cancellationToken).ConfigureAwait(false);
            }

            return await OnNextActionAsync(dc, result, cancellationToken).ConfigureAwait(false);
        }

        public virtual IEnumerable<Dialog> GetDependencies()
        {
            foreach (var action in Actions)
            {
                yield return action;
            }
        }

        protected virtual async Task<DialogTurnResult> OnActionScopeResultAsync(DialogContext dc, ActionScopeResult actionScopeResult, CancellationToken cancellationToken = default)
        {
            switch (actionScopeResult.ActionScopeCommand)
            {
                case ActionScopeCommands.GotoAction:
                    return await this.OnGotoActionAsync(dc, actionScopeResult, cancellationToken).ConfigureAwait(false);

                case ActionScopeCommands.BreakLoop:
                    return await this.OnBreakLoopAsync(dc, actionScopeResult, cancellationToken).ConfigureAwait(false);

                case ActionScopeCommands.ContinueLoop:
                    return await this.OnContinueLoopAsync(dc, actionScopeResult, cancellationToken).ConfigureAwait(false);

                default:
                    throw new NotImplementedException($"Unknown action scope command returned: {actionScopeResult.ActionScopeCommand}");
            }
        }

        protected virtual async Task<DialogTurnResult> OnGotoActionAsync(DialogContext dc, ActionScopeResult actionScopeResult, CancellationToken cancellationToken = default)
        {
            // Look for action to goto in our scope
            var offset = this.Actions.FindIndex((d) => d.Id == actionScopeResult.ActionId);

            // Is this a action Id for us?
            if (offset >= 0)
            {
                // begin that action
                return await this.BeginActionAsync(dc, offset, cancellationToken).ConfigureAwait(false);
            }
            else if (dc.Stack.Count > 1)
            {
                // send it to parent to resolve
                return await dc.EndDialogAsync(actionScopeResult, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // we have not found the goto id.
                throw new Exception($"GotoAction: could not find an action of '{actionScopeResult.ActionId}'.");
            }
        }

        protected virtual async Task<DialogTurnResult> OnBreakLoopAsync(DialogContext dc, ActionScopeResult actionScopeResult, CancellationToken cancellationToken = default)
        {
            // default is to simply end the dialog and propagate to parent to handle
            return await dc.EndDialogAsync(actionScopeResult, cancellationToken).ConfigureAwait(false);
        }

        protected virtual async Task<DialogTurnResult> OnContinueLoopAsync(DialogContext dc, ActionScopeResult actionScopeResult, CancellationToken cancellationToken = default)
        {
            // default is to simply end the dialog and propagate to parent to handle
            return await dc.EndDialogAsync(actionScopeResult, cancellationToken).ConfigureAwait(false);
        }

        protected virtual async Task<DialogTurnResult> OnNextActionAsync(DialogContext dc, object result = null, CancellationToken cancellationToken = default)
        {
            // Check for any plan changes
            var hasChanges = false;
            var root = dc;
            var parent = dc;
            while (parent != null)
            {
                var ac = parent as ActionContext;
                if (ac != null && ac.Changes != null && ac.Changes.Count > 0)
                {
                    hasChanges = true;
                }

                root = parent;
                parent = root.Parent;
            }

            // Apply any changes
            if (hasChanges)
            {
                // Recursively call ContinueDialogAsync() to apply changes and continue execution.
                return await root.ContinueDialogAsync(cancellationToken).ConfigureAwait(false);
            }

            // Increment our offset into the actions and being the next action
            var nextOffset = dc.State.GetIntValue(OFFSETKEY, 0) + 1;
            if (nextOffset < this.Actions.Count)
            {
                return await this.BeginActionAsync(dc, nextOffset, cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            // else we fire the end of actions
            return await this.OnEndOfActionsAsync(dc, result, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected virtual async Task<DialogTurnResult> OnEndOfActionsAsync(DialogContext dc, object result = null, CancellationToken cancellationToken = default)
        {
            if (result is CancellationToken)
            {
                throw new ArgumentException($"{nameof(result)} cannot be a cancellation token");
            }

            // default for end of actions is to end the action scope by ending the dialog
            return await dc.EndDialogAsync(result, cancellationToken).ConfigureAwait(false);
        }

        protected virtual async Task<DialogTurnResult> BeginActionAsync(DialogContext dc, int offset, CancellationToken cancellationToken = default)
        {
            // get the action for the offset
            dc.State.SetValue(OFFSETKEY, offset);
            var action = this.Actions[offset];
            var actionName = action.GetType().Name.ToString();

            var properties = new Dictionary<string, string>()
            {
                { "DialogId", action.Id },
                { "Kind", $"Microsoft.{actionName}" },
                { "Instance", JsonConvert.SerializeObject(action, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }).ToString() }
            };
            TelemetryClient.TrackEvent("AdaptiveDialogAction", properties);

            // begin Action dialog
            return await dc.BeginDialogAsync(action.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"ActionScope[{string.Join(",", Actions.Select(a => a.Id))}]";
        }
    }
}
