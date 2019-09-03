// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// How to modify an action sequence.
    /// </summary>
    public enum ActionChangeType
    {
        /// <summary>
        /// Add the change actions to the head of the sequence.
        /// </summary>
        InsertActions,

        /// <summary>
        /// Insert the change actions before named tags in the sequence.
        /// </summary>
        InsertActionsBeforeTags,

        /// <summary>
        /// Add the changeactions to the tail of the sequence.
        /// </summary>
        AppendActions,

        /// <summary>
        /// Terminate the action sequence.
        /// </summary>
        EndSequence,

        /// <summary>
        /// Terminate the action sequence, then add the change actions.
        /// </summary>
        ReplaceSequence,
    }

    public class SequenceContext : DialogContext
    {
        private readonly string changeKey;

        private DialogSet actionDialogs;

        public SequenceContext(DialogSet dialogs, DialogContext dc, DialogState state, List<ActionState> actions, string changeKey, DialogSet actionDialogs)
            : base(dialogs, dc.Context, state, conversationState: dc.State.Conversation, userState: dc.State.User, settings: dc.State.Settings)
        {
            this.Actions = actions;
            this.changeKey = changeKey;
            this.actionDialogs = actionDialogs;
        }

        public AdaptiveDialogState Plans { get; private set; }

        /// <summary>
        /// Gets or sets list of actions being executed.
        /// </summary>
        /// <value>
        /// List of actions being executed.
        /// </value>
        public List<ActionState> Actions { get; set; }

        /// <summary>
        /// Gets list of changes that are queued to be applied.
        /// </summary>
        /// <value>
        /// List of changes that are queued to be applied.
        /// </value>
        public List<ActionChangeList> Changes
        {
            get { return this.Context.TurnState.Get<List<ActionChangeList>>(changeKey); }
            private set { this.Context.TurnState[changeKey] = value; }
        }

        /// <summary>
        /// Queues up a set of changes that will be applied when ApplyChanges is called.
        /// </summary>
        /// <param name="changes">Plan changes to queue up.</param>
        public void QueueChanges(ActionChangeList changes)
        {
            // Pull change lists from turn context
            var queue = this.Changes ?? new List<ActionChangeList>();
            queue.Add(changes);

            // Save back changes to turn context
            Changes = queue;
        }

        /// <summary>
        /// Applies any queued up changes.
        /// </summary>
        /// <remarks>
        /// Applying a set of changes can result in additional changes being queued. The method
        /// will loop and apply any additional plan changes until there are no more changes left to 
        /// apply.
        /// </remarks>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if there were any changes to apply. </returns>
        public async Task<bool> ApplyChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            // Retrieve queued changes from turn context
            var changes = this.Changes ?? new List<ActionChangeList>();

            if (changes.Any())
            {
                // Clear current change list
                this.Context.TurnState[changeKey] = null;
                
                // Apply each queued set of changes
                foreach (var change in changes)
                {
                    // Apply memory changes to turn state
                    if (change.Turn != null)
                    {
                        foreach (var keyValue in change.Turn)
                        {
                            this.State.SetValue($"turn.{keyValue.Key}", keyValue.Value);
                        }
                    }

                    switch (change.ChangeType)
                    {
                        case ActionChangeType.InsertActions:
                        case ActionChangeType.InsertActionsBeforeTags:
                        case ActionChangeType.AppendActions:
                            await UpdateSequenceAsync(change, cancellationToken).ConfigureAwait(false);
                            break;
                        case ActionChangeType.EndSequence:
                            if (this.Actions.Any())
                            {
                                this.Actions.Clear();
                            }

                            break;
                        case ActionChangeType.ReplaceSequence:
                            if (this.Actions.Any())
                            {
                                this.Actions.Clear();
                            }

                            await UpdateSequenceAsync(change, cancellationToken).ConfigureAwait(false);
                            break;
                    }
                }

                // Apply any queued up changes
                await ApplyChangesAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }

            return false;
        }

        public SequenceContext InsertActions(List<ActionState> actions)
        {
            this.QueueChanges(
                new ActionChangeList()
                {
                    ChangeType = ActionChangeType.InsertActions,
                    Actions = actions
                });
            return this;
        }

        public SequenceContext InsertActionsBeforeTags(List<string> tags, List<ActionState> actions)
        {
            this.QueueChanges(
                new ActionChangeList()
                {
                    ChangeType = ActionChangeType.InsertActionsBeforeTags,
                    Actions = actions,
                    Tags = tags
                });
            return this;
        }

        public SequenceContext AppendActions(List<ActionState> actions)
        {
            this.QueueChanges(
                new ActionChangeList()
                {
                    ChangeType = ActionChangeType.AppendActions,
                    Actions = actions
                });
            return this;
        }

        public SequenceContext EndSequence(List<ActionState> actions)
        {
            this.QueueChanges(
                new ActionChangeList()
                {
                    ChangeType = ActionChangeType.EndSequence,
                    Actions = actions
                });
            return this;
        }

        public SequenceContext ReplaceSequence(List<ActionState> actions)
        {
            this.QueueChanges(
                new ActionChangeList()
                {
                    ChangeType = ActionChangeType.ReplaceSequence,
                    Actions = actions
                });
            return this;
        }

        /// <summary>
        /// Specifies whether a given dialog should inherit dialog-level state. For adaptive dialogs, 
        /// we take our base class cases plus we explicitly ask that InputDialogs inherit state as well.
        /// InputDialogs don't inherit state out of the box because they inherit directly from Dialog and 
        /// are declared in the Adaptive assembly, so the base class, DialogContext does not explicitly
        /// request that they inherit state. Thus, we add it here. This enables seamless usage of
        /// dialog level properties such as $name across Input dialogs and / or steps within an adaptive dialog.
        /// </summary>
        /// <param name="dialog">The dialog to be tested.</param>
        /// <returns>Whether the passed dialog should inherit dialog-level state.</returns>
        protected override bool ShouldInheritState(IDialog dialog)
        {
            return base.ShouldInheritState(dialog) || dialog is InputDialog;
        }

        private async Task UpdateSequenceAsync(ActionChangeList change, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (change == null)
            {
                throw new ArgumentNullException(nameof(change));
            }

            // Initialize sequence if needed
            var newSequence = !this.Actions.Any();

            // Update sequence
            switch (change.ChangeType)
            {
                case ActionChangeType.InsertActions:
                    // Insert at the beginning, being careful to not change the reference to this.Actions instance,
                    // since it is tied to the state.
                    var newActions = new List<ActionState>(change.Actions);
                    newActions.AddRange(this.Actions);
                    this.Actions.Clear();
                    this.Actions.AddRange(newActions);
                    break;

                case ActionChangeType.InsertActionsBeforeTags:
                    var inserted = false;

                    if (change.Tags != null && change.Tags.Any())
                    {
                        // Walk list of actions to find point at which to insert new actions based off tags
                        for (int i = 0; i < this.Actions.Count; i++)
                        {
                            // Does the current step have one of the tags we are looking for?
                            if (ActionHasTags(this.Actions[i], change.Tags))
                            {
                                // Insert actions before the current step
                                // We have actions before and after the insertion point, and we want to insert the change
                                // actions in the middle
                                this.Actions.InsertRange(i, change.Actions);
                                inserted = true;
                                break;
                            }
                        }
                    }

                    // If we didn't find any of the tags we were looking for, then just
                    // append the actions to the end of the current sequence
                    if (!inserted)
                    {
                        this.Actions.AddRange(change.Actions);
                    }

                    break;

                case ActionChangeType.AppendActions:
                case ActionChangeType.ReplaceSequence:
                    this.Actions.AddRange(change.Actions);
                    break;
            }
        }

        private bool ActionHasTags(ActionState step, List<string> tags)
        {
            var dialog = actionDialogs.Find(step.DialogId);
            if (dialog != null && dialog.Tags != null)
            {
                // True if the dialog contains any of the tags passed as parameters
                return tags.Any(t => dialog.Tags.Contains(t));
            }

            return false;
        }
    }

    public class AdaptiveEvents : DialogContext.DialogEvents
    {
        public const string RecognizedIntent = "recognizedIntent";
        public const string UnknownIntent = "unknownIntent";
    }

    public class AdaptiveDialogState
    {
        public AdaptiveDialogState()
        {
        }

        [JsonProperty(PropertyName = "options")]
        public dynamic Options { get; set; }

        [JsonProperty(PropertyName = "actions")]
        public List<ActionState> Actions { get; set; } = new List<ActionState>();

        [JsonProperty(PropertyName = "result")]
        public object Result { get; set; }
    }

    [DebuggerDisplay("{DialogId}")]
    public class ActionState : DialogState
    {
        public ActionState()
        {
        }

        public ActionState(string dialogId = null, object options = null)
        {
            DialogId = dialogId;
            Options = options;
        }

        [JsonProperty(PropertyName = "dialogId")]
        public string DialogId { get; set; }

        [JsonProperty(PropertyName = "options")]
        public object Options { get; set; }
    }

    [DebuggerDisplay("{ChangeType}:{Desire}")]
    public class ActionChangeList
    {
        [JsonProperty(PropertyName = "changeType")]
        public ActionChangeType ChangeType { get; set; } = ActionChangeType.InsertActions;

        [JsonProperty(PropertyName = "actions")]
        public List<ActionState> Actions { get; set; } = new List<ActionState>();

        [JsonProperty(PropertyName = "tags")]
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets turn state associated with the plan change list (it will be applied to turn state when plan is applied).
        /// </summary>
        /// <value>
        /// Turn state associated with the plan change list (it will be applied to turn state when plan is applied).
        /// </value>
        [JsonProperty(PropertyName = "turn")]
        public Dictionary<string, object> Turn { get; set; }
    }
}
