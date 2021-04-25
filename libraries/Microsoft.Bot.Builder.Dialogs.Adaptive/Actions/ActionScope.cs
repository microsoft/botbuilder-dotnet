// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// ActionScope manages execution of a block of actions, and supports Goto, Continue and Break semantics..
    /// </summary>
    public class ActionScope : Dialog, IDialogDependencies
    {
        /// <summary>
        /// Defines the path for the offset key.
        /// </summary>
        protected const string OFFSETKEY = "this.offset";

        private List<Dialog> actions = new List<Dialog>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionScope"/> class.
        /// </summary>
        /// <param name="actions">The actions to execute.</param>
        public ActionScope(IEnumerable<Dialog> actions = null)
        {
            if (actions != null)
            {
                this.actions = new List<Dialog>(actions);
            }
        }

        /// <summary>
        /// Gets or sets the actions to execute.
        /// </summary>
        /// <value>The actions to execute.</value>
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public List<Dialog> Actions
        {
            get
            {
                return this.actions;
            }

            set
            {
                this.actions = value ?? new List<Dialog>();
            }
        }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Called when the dialog is started and pushed onto the dialog stack.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="options">Optional, initial information to pass to the dialog.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Called when the dialog is _continued_, where it is the active dialog and the
        /// user replies with a new activity.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            // We're being continued after an interruption so just run next action
            return await OnNextActionAsync(dc, null, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Called when a child dialog completed its turn, returning control to this dialog.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of the conversation.</param>
        /// <param name="reason">Reason why the dialog resumed.</param>
        /// <param name="result">Optional, value returned from the dialog that was called. The type
        /// of the value returned is dependent on the child dialog.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default)
        {
            if (result is ActionScopeResult actionScopeResult)
            {
                return await OnActionScopeResultAsync(dc, actionScopeResult, cancellationToken).ConfigureAwait(false);
            }

            return await OnNextActionAsync(dc, result, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a unique string which represents the version of this dialog. If the version
        /// changes between turns the dialog system will emit a DialogChanged event.
        /// </summary>
        /// <returns>Unique string which should only change when dialog has changed in a
        /// way that should restart the dialog.</returns>
        public override string GetVersion()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var action in this.Actions)
            {
                var v = action.GetVersion();
                if (v != null)
                {
                    sb.Append(v);
                }
            }

            return StringUtils.Hash(sb.ToString());
        }

        /// <summary>
        /// Enumerates child dialog dependencies so they can be added to the containers dialog set.
        /// </summary>
        /// <returns>Dialog enumeration.</returns>
        public virtual IEnumerable<Dialog> GetDependencies()
        {
            foreach (var action in Actions)
            {
                yield return action;
            }
        }

        /// <summary>
        /// Called when returning control to this dialog with an <see cref="ActionScopeResult"/>.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of the conversation.</param>
        /// <param name="actionScopeResult">Contains the actions scope result.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Called when returning control to this dialog with an <see cref="ActionScopeResult"/>
        /// with the property ActionCommand set to <c>GoToAction</c>.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of the conversation.</param>
        /// <param name="actionScopeResult">Contains the actions scope result.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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
                throw new ArgumentException($"GotoAction: could not find an action of \"{actionScopeResult.ActionId}\".");
            }
        }

        /// <summary>
        /// Called when returning control to this dialog with an <see cref="ActionScopeResult"/>
        /// with the property ActionCommand set to <c>BreakLoop</c>.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of the conversation.</param>
        /// <param name="actionScopeResult">Contains the actions scope result.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected virtual async Task<DialogTurnResult> OnBreakLoopAsync(DialogContext dc, ActionScopeResult actionScopeResult, CancellationToken cancellationToken = default)
        {
            // default is to simply end the dialog and propagate to parent to handle
            return await dc.EndDialogAsync(actionScopeResult, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Called when returning control to this dialog with an <see cref="ActionScopeResult"/>
        /// with the property ActionCommand set to <c>ContinueLoop</c>.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of the conversation.</param>
        /// <param name="actionScopeResult">Contains the actions scope result.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>Default is to simply end the dialog and propagate to parent to handle.</remarks>
        protected virtual async Task<DialogTurnResult> OnContinueLoopAsync(DialogContext dc, ActionScopeResult actionScopeResult, CancellationToken cancellationToken = default)
        {
            // default is to simply end the dialog and propagate to parent to handle
            return await dc.EndDialogAsync(actionScopeResult, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Called when the dialog continues to the next action.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="result">Optional, value returned from the dialog that was called. The type
        /// of the value returned is dependent on the child dialog.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Called when the dialog's action ends.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="result">Optional, value returned from the dialog that was called. The type
        /// of the value returned is dependent on the child dialog.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected virtual async Task<DialogTurnResult> OnEndOfActionsAsync(DialogContext dc, object result = null, CancellationToken cancellationToken = default)
        {
            if (result is CancellationToken)
            {
                throw new ArgumentException($"{nameof(result)} cannot be a cancellation token");
            }

            // default for end of actions is to end the action scope by ending the dialog
            return await dc.EndDialogAsync(result, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Starts a new dialog and pushes it onto the dialog stack.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="offset">Optional, value returned from the dialog that was called. The type
        /// of the value returned is dependent on the child dialog.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected virtual async Task<DialogTurnResult> BeginActionAsync(DialogContext dc, int offset, CancellationToken cancellationToken = default)
        {
            // get the action for the offset
            dc.State.SetValue(OFFSETKEY, offset);

            if (this.Actions == null || this.Actions.Count <= offset)
            {
                return await dc.EndDialogAsync(null, cancellationToken).ConfigureAwait(false);
            }

            var action = this.Actions[offset];
            var actionName = action.GetType().Name.ToString();

            var properties = new Dictionary<string, string>()
            {
                { "DialogId", action.Id },
                { "Kind", $"Microsoft.{actionName}" },
                { "ActionId", $"Microsoft.{action.Id}" },
            };
            TelemetryClient.TrackEvent("AdaptiveDialogAction", properties);

            // begin Action dialog
            return await dc.BeginDialogAsync(action.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override string OnComputeId()
        {
            return $"ActionScope[{StringUtils.EllipsisHash(string.Join(",", Actions.Select(a => a.Id)), 50)}]";
        }
    }
}
