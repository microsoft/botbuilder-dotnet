// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Dialog optimized for prompting a user with a series of questions. Waterfalls accept a stack of
    /// functions which will be executed in sequence.Each waterfall step can ask a question of the user
    /// and the users response will be passed as an argument to the next waterfall step.
    /// </summary>
    public class WaterfallDialog : Dialog
    {
        private const string PersistedOptions = "options";
        private const string StepIndex = "stepIndex";
        private const string PersistedValues = "values";

        private WaterfallStep[] _steps;

        public WaterfallDialog(string dialogId, WaterfallStep[] steps = null)
            : base(dialogId)
        {
            _steps = steps ?? new WaterfallStep[] { };
        }

        public override async Task<DialogStatus> DialogBeginAsync(DialogContext dc, DialogOptions options = null)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            // Initialize waterfall state
            var state = dc.ActiveDialog.State;
            state[PersistedOptions] = options;
            state[PersistedValues] = new Dictionary<string, object>();

            // Run first step
            return await RunStepAsync(dc, 0, DialogReason.BeginCalled).ConfigureAwait(false);
        }

        public override async Task<DialogStatus> DialogContinueAsync(DialogContext dc)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            // Don't do anything for non-message activities.
            if (dc.Context.Activity.Type != ActivityTypes.Message)
            {
                return DialogStatus.Waiting;
            }

            // Run next step with the message text as the result.
            return await DialogResumeAsync(dc, DialogReason.ContinueCalled, dc.Context.Activity.Text).ConfigureAwait(false);
        }

        public override async Task<DialogStatus> DialogResumeAsync(DialogContext dc, DialogReason reason, object result)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            // Increment step index and run step
            var state = dc.ActiveDialog.State;
            var index = (int)state[StepIndex];
            return await RunStepAsync(dc, index + 1, reason, result).ConfigureAwait(false);
        }

        protected virtual async Task<DialogStatus> OnStepAsync(DialogContext dc, WaterfallStepContext step)
        {
            return await _steps[step.Index](dc, step).ConfigureAwait(false);
        }

        private async Task<DialogStatus> RunStepAsync(DialogContext dc, int index, DialogReason reason, object result = null)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            if (index < _steps.Length)
            {
                // Update persisted step index
                var state = dc.ActiveDialog.State;
                state[StepIndex] = index;

                // Create step context
                var options = (DialogOptions)state[PersistedOptions];
                var values = (IDictionary<string, object>)state[PersistedValues];
                var step = new WaterfallStepContext(this, dc, options, values, index, reason, result);

                // Execute step
                return await OnStepAsync(dc, step).ConfigureAwait(false);
            }
            else
            {
                // End of waterfall so just return any result to parent
                return await dc.EndAsync(result).ConfigureAwait(false);
            }
        }
    }
}
