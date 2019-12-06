using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
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

        public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default)
        {
            if (result is ActionScopeResult actionScopeResult)
            {
                return await OnActionScopeResultAsync(dc, actionScopeResult, cancellationToken).ConfigureAwait(false);
            }

            var nextOffset = dc.GetState().GetIntValue(OFFSETKEY, 0) + 1;
            if (nextOffset < this.Actions.Count)
            {
                return await this.BeginActionAsync(dc, nextOffset, cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            return await this.OnEndOfActionsAsync(dc, result, cancellationToken: cancellationToken).ConfigureAwait(false);
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
            // Look for action to goto
            var offset = this.Actions.FindIndex((d) => d.Id == actionScopeResult.ActionId);

            // Is this a label for us?
            if (offset >= 0)
            {
                // being that action
                return await this.BeginActionAsync(dc, offset, cancellationToken).ConfigureAwait(false);
            }
            else if (dc.Stack.Count > 1)
            {
                return await dc.EndDialogAsync(actionScopeResult, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new Exception($"GotoAction: could not find an action of '{actionScopeResult.ActionId}'.");
            }
        }

        protected virtual async Task<DialogTurnResult> OnBreakLoopAsync(DialogContext dc, ActionScopeResult actionScopeResult, CancellationToken cancellationToken = default)
        {
            return await dc.EndDialogAsync(actionScopeResult, cancellationToken).ConfigureAwait(false);
        }

        protected virtual async Task<DialogTurnResult> OnContinueLoopAsync(DialogContext dc, ActionScopeResult actionScopeResult, CancellationToken cancellationToken = default)
        {
            return await dc.EndDialogAsync(actionScopeResult, cancellationToken).ConfigureAwait(false);
        }

        protected virtual async Task<DialogTurnResult> OnEndOfActionsAsync(DialogContext dc, object result = null, CancellationToken cancellationToken = default)
        {
            if (result is CancellationToken)
            {
                throw new ArgumentException($"{nameof(result)} cannot be a cancellation token");
            }

            return await dc.EndDialogAsync(result, cancellationToken).ConfigureAwait(false);
        }

        protected virtual async Task<DialogTurnResult> BeginActionAsync(DialogContext dc, int offset, CancellationToken cancellationToken = default)
        {
            dc.GetState().SetValue(OFFSETKEY, offset);
            var actionId = this.Actions[offset].Id;

            // begin Action
            return await dc.BeginDialogAsync(actionId, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"ActionScope[{string.Join(",", Actions.Select(a => a.Id))}]";
        }
    }
}
