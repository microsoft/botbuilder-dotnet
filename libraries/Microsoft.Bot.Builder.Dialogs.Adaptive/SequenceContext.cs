// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    public class SequenceContext : DialogContext
    {
        private readonly string changeKey;

        public SequenceContext(DialogSet dialogs, DialogContext dc, DialogState state, List<ActionState> actions, string changeKey)
            : base(dialogs, dc, state)
        {
            this.Actions = actions;
            this.changeKey = changeKey;
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
            var dcState = this.GetState();

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
                            dcState.SetValue($"turn.{keyValue.Key}", keyValue.Value);
                        }
                    }

                    // Update sequence
                    switch (change.ChangeType)
                    {
                        case ActionChangeType.InsertActions:
                            this.Actions.InsertRange(0, change.Actions);
                            break;

                        case ActionChangeType.AppendActions:
                            this.Actions.AddRange(change.Actions);
                            break;

                        case ActionChangeType.EndSequence:
                            this.Actions.Clear();
                            break;

                        case ActionChangeType.ReplaceSequence:
                            this.Actions.Clear();
                            this.Actions.AddRange(change.Actions);
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
    }
}
