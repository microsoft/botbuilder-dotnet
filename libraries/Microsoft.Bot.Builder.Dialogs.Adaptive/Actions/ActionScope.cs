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

        public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            if (this.Actions.Any())
            {
                return this.BeginActionAsync(dc, 0, cancellationToken);
            }
            else
            {
                return dc.EndDialogAsync(null, cancellationToken);
            }
        }

        public override Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default)
        {
            if (result is ActionScopeResult actionScopeResult)
            {
                return OnActionScopeResultAsync(dc, actionScopeResult, cancellationToken);
            }

            var nextOffset = dc.GetState().GetIntValue(OFFSETKEY, 0) + 1;
            if (nextOffset < this.Actions.Count)
            {
                return this.BeginActionAsync(dc, nextOffset, cancellationToken: cancellationToken);
            }

            return this.OnEndOfActionsAsync(dc, result, cancellationToken: cancellationToken);
        }

        public virtual IEnumerable<Dialog> GetDependencies()
        {
            foreach (var action in Actions)
            {
                yield return action;
            }
        }

        protected virtual Task<DialogTurnResult> OnActionScopeResultAsync(DialogContext dc, ActionScopeResult actionScopeResult, CancellationToken cancellationToken = default)
        {
            switch (actionScopeResult.ActionScopeCommand)
            {
                case ActionScopeCommands.GotoAction:
                    return this.OnGotoActionAsync(dc, actionScopeResult, cancellationToken);

                case ActionScopeCommands.BreakLoop:
                    return this.OnBreakLoopAsync(dc, actionScopeResult, cancellationToken);

                case ActionScopeCommands.ContinueLoop:
                    return this.OnContinueLoopAsync(dc, actionScopeResult, cancellationToken);

                default:
                    throw new NotImplementedException($"Unknown action scope command returned: {actionScopeResult.ActionScopeCommand}");
            }
        }

        protected virtual Task<DialogTurnResult> OnGotoActionAsync(DialogContext dc, ActionScopeResult actionScopeResult, CancellationToken cancellationToken = default)
        {
            // Look for action to goto
            var offset = this.Actions.FindIndex((d) => d.Id == actionScopeResult.ActionId);

            // Is this a label for us?
            if (offset >= 0)
            {
                // being that action
                return this.BeginActionAsync(dc, offset, cancellationToken);
            }
            else if (dc.Stack.Count > 1)
            {
                return dc.EndDialogAsync(actionScopeResult, cancellationToken);
            }
            else
            {
                throw new Exception($"GotoAction: could not find an action of '{actionScopeResult.ActionId}'.");
            }
        }

        protected virtual Task<DialogTurnResult> OnBreakLoopAsync(DialogContext dc, ActionScopeResult actionScopeResult, CancellationToken cancellationToken = default)
        {
            return dc.EndDialogAsync(actionScopeResult, cancellationToken);
        }

        protected virtual Task<DialogTurnResult> OnContinueLoopAsync(DialogContext dc, ActionScopeResult actionScopeResult, CancellationToken cancellationToken = default)
        {
            return dc.EndDialogAsync(actionScopeResult, cancellationToken);
        }

        protected virtual Task<DialogTurnResult> OnEndOfActionsAsync(DialogContext dc, object result = null, CancellationToken cancellationToken = default)
        {
            if (result is CancellationToken)
            {
                throw new ArgumentException($"{nameof(result)} cannot be a cancellation token");
            }

            return dc.EndDialogAsync(result, cancellationToken);
        }

        protected virtual Task<DialogTurnResult> BeginActionAsync(DialogContext dc, int offset, CancellationToken cancellationToken = default)
        {
            dc.GetState().SetValue(OFFSETKEY, offset);
            var actionId = this.Actions[offset].Id;

            // begin Action
            return dc.BeginDialogAsync(actionId, cancellationToken: cancellationToken);
        }

        protected override string OnComputeId()
        {
            return $"ActionScope[{string.Join(",", Actions.Select(a => a.Id))}]";
        }
    }
}
