// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    public class SequenceContext : DialogContext
    {
        private readonly string changeKey;

        public AdaptiveDialogState Plans { get; private set; }

        /// <summary>
        /// List of steps being executed.
        /// </summary>
        public List<StepState> Steps { get; set; }
        
        /// <summary>
        /// List of changes that are queued to be applied.
        /// </summary>
        public List<StepChangeList> Changes
        {
            get { return this.Context.TurnState.Get<List<StepChangeList>>(changeKey); }
            private set { this.Context.TurnState[changeKey] = value; }
        }

        public SequenceContext(DialogSet dialogs, DialogContext dc, DialogState state, List<StepState> steps, string changeKey)
            : base(dialogs, dc.Context, state, conversationState: dc.State.Conversation, userState: dc.State.User, settings: dc.State.Settings)
        {
            this.Steps = steps;
            this.changeKey = changeKey;
        }

        /// <summary>
        /// Queues up a set of changes that will be applied when ApplyChanges is called.
        /// </summary>
        /// <param name="changes">Plan changes to queue up</param>
        public void QueueChanges(StepChangeList changes)
        {
            // Pull change lists from turn context
            var queue = this.Changes ?? new List<StepChangeList>();
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
        /// <returns>True if there were any changes to apply. </returns>
        public async Task<bool> ApplyChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            // Retrieve queued changes from turn context
            var changes = this.Changes ?? new List<StepChangeList>();

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
                        case StepChangeTypes.InsertSteps:
                        case StepChangeTypes.InsertStepsBeforeTags:
                        case StepChangeTypes.AppendSteps:
                            await UpdateSequenceAsync(change, cancellationToken).ConfigureAwait(false);
                            break;
                        case StepChangeTypes.EndSequence:
                            if (this.Steps.Any())
                            {
                                this.Steps.Clear();
                            }
                            await EmitEventAsync(name: AdaptiveEvents.SequenceEnded, value: null, bubble: false).ConfigureAwait(false);
                            break;
                        case StepChangeTypes.ReplaceSequence:
                            if (this.Steps.Any())
                            {
                                this.Steps.Clear();
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

        public SequenceContext InsertSteps(List<StepState> steps)
        {
            this.QueueChanges(
                new StepChangeList()
                {
                    ChangeType = StepChangeTypes.InsertSteps,
                    Steps = steps
                });
            return this;
        }

        public SequenceContext InsertStepsBeforeTags(List<string> tags, List<StepState> steps)
        {
            this.QueueChanges(
                new StepChangeList()
                {
                    ChangeType = StepChangeTypes.InsertStepsBeforeTags,
                    Steps = steps,
                    Tags = tags
                });
            return this;
        }

        public SequenceContext AppendSteps(List<StepState> steps)
        {
            this.QueueChanges(
                new StepChangeList()
                {
                    ChangeType = StepChangeTypes.AppendSteps,
                    Steps = steps
                });
            return this;
        }

        public SequenceContext EndSequence(List<StepState> steps)
        {
            this.QueueChanges(
                new StepChangeList()
                {
                    ChangeType = StepChangeTypes.EndSequence,
                    Steps = steps
                });
            return this;
        }

        public SequenceContext ReplaceSequence(List<StepState> steps)
        {
            this.QueueChanges(
                new StepChangeList()
                {
                    ChangeType = StepChangeTypes.ReplaceSequence,
                    Steps = steps
                });
            return this;
        }

        private async Task UpdateSequenceAsync(StepChangeList change, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (change == null)
            {
                throw new ArgumentNullException(nameof(change));
            }

            // Initialize sequence if needed
            var newSequence = !this.Steps.Any();

            // Update sequence
            switch (change.ChangeType)
            {
                case StepChangeTypes.InsertSteps:
                    // Insert at the beginning, being careful to not change the reference to this.Steps instance,
                    // since it is tied to the state.
                    var newSteps = new List<StepState>(change.Steps);
                    newSteps.AddRange(this.Steps);
                    this.Steps.Clear();
                    this.Steps.AddRange(newSteps);
                    break;

                case StepChangeTypes.InsertStepsBeforeTags:
                    var inserted = false;

                    if (change.Tags != null && change.Tags.Any())
                    {
                        // Walk list of steps to find point at which to insert new steps based off tags
                        for (int i = 0; i < this.Steps.Count; i++)
                        {
                            // Does the current step have one of the tags we are looking for?
                            if (StepHasTags(this.Steps[i], change.Tags))
                            {
                                // Insert steps before the current step
                                // We have steps before and after the insertion point, and we want to insert the change
                                // steps in the middle
                                this.Steps.InsertRange(i, change.Steps);
                                inserted = true;
                                break;
                            }
                        }
                    }

                    // If we didn't find any of the tags we were looking for, then just
                    // append the steps to the end of the current sequence
                    if (!inserted)
                    {
                        this.Steps.AddRange(change.Steps);
                    }

                    break;

                case StepChangeTypes.AppendSteps:
                case StepChangeTypes.ReplaceSequence:
                    this.Steps.AddRange(change.Steps);
                    break;
            }

            // Emit SequenceStarted event if applicable
            if (newSequence)
            {
                await this.EmitEventAsync(name: AdaptiveEvents.SequenceStarted, value: null, bubble: false).ConfigureAwait(false);
            }
        }

        private bool StepHasTags(StepState step, List<string> tags)
        {
            var dialog = this.FindDialog(step.DialogId);

            if (dialog != null && dialog.Tags != null)
            {
                // True if the dialog contains any of the tags passed as parameters
                return tags.Any(t => dialog.Tags.Contains(t));
            }

            return false;
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
    }

    public class AdaptiveEvents : DialogContext.DialogEvents
    {
        public const string RecognizedIntent = "recognizedIntent";
        public const string UnknownIntent = "unknownIntent";
        public const string SequenceStarted = "stepsStarted";
        public const string SequenceEnded = "stepsEnded";
    }

    public class AdaptiveDialogState
    {
        public AdaptiveDialogState()
        {
        }

        [JsonProperty(PropertyName = "options")]
        public dynamic Options { get; set; }

        [JsonProperty(PropertyName = "steps")]
        public List<StepState> Steps { get; set; } = new List<StepState>();

        [JsonProperty(PropertyName = "result")]
        public object Result { get; set; }
    }

    [DebuggerDisplay("{DialogId}")]
    public class StepState : DialogState
    {
        public StepState()
        {
        }

        public StepState(string dialogId = null, object options = null)
        {
            DialogId = dialogId;
            Options = options;
        }

        [JsonProperty(PropertyName = "dialogId")]
        public string DialogId { get; set; }

        [JsonProperty(PropertyName = "options")]
        public object Options { get; set; }
    }

    public enum StepChangeTypes
    {
        InsertSteps,
        InsertStepsBeforeTags,
        AppendSteps,
        EndSequence,
        ReplaceSequence,
    }

    [DebuggerDisplay("{ChangeType}:{Desire}")]
    public class StepChangeList
    {
        [JsonProperty(PropertyName = "changeType")]
        public StepChangeTypes ChangeType { get; set; } = StepChangeTypes.InsertSteps;

        [JsonProperty(PropertyName = "steps")]
        public List<StepState> Steps { get; set; } = new List<StepState>();

        [JsonProperty(PropertyName = "tags")]
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets turn state associated with the plan change list (it will be applied to turn state when plan is applied)
        /// </summary>
        [JsonProperty(PropertyName = "turn")]
        public Dictionary<string, object> Turn { get; set; }
    }
}
