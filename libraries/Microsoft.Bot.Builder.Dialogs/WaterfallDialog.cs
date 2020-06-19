// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Dialog optimized for prompting a user with a series of questions. Waterfalls accept a stack of
    /// functions which will be executed in sequence. Each waterfall step can ask a question of the user
    /// and the user's response will be passed as an argument to the next waterfall step.
    /// </summary>
    public class WaterfallDialog : Dialog
    {
        private const string PersistedOptions = "options";
        private const string StepIndex = "stepIndex";
        private const string PersistedValues = "values";
        private const string PersistedInstanceId = "instanceId";

        private readonly List<WaterfallStep> _steps;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaterfallDialog"/> class.
        /// </summary>
        /// <param name="dialogId">The dialog ID.</param>
        /// <param name="actions">Optional actions to be defined by the caller.</param>
        public WaterfallDialog(string dialogId, IEnumerable<WaterfallStep> actions = null)
            : base(dialogId)
        {
            if (actions != null)
            {
                _steps = new List<WaterfallStep>(actions);
            }
            else
            {
                _steps = new List<WaterfallStep>();
            }
        }

        /// <summary>
        /// Gets a unique string which represents the version of this dialog.  If the version changes between turns the dialog system will emit a DialogChanged event.
        /// </summary>
        /// <returns>Version will change when steps count changes (because dialog has no way of evaluating the content of the steps.</returns>
        public override string GetVersion()
        {
            return $"{this.Id}:{_steps.Count}";
        }

        /// <summary>
        /// Adds a new step to the waterfall.
        /// </summary>
        /// <param name="step">Step to add.</param>
        /// <returns>Waterfall dialog for fluent calls to `AddStep()`.</returns>
        public WaterfallDialog AddStep(WaterfallStep step)
        {
            _steps.Add(step ?? throw new ArgumentNullException(nameof(step)));
            return this;
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            // Initialize waterfall state
            var state = dc.ActiveDialog.State;
            var instanceId = Guid.NewGuid().ToString();
            state[PersistedOptions] = options;
            state[PersistedValues] = new Dictionary<string, object>();
            state[PersistedInstanceId] = instanceId;

            var properties = new Dictionary<string, string>()
                {
                    { "DialogId", Id },
                    { "InstanceId", instanceId },
                };
            TelemetryClient.TrackEvent("WaterfallStart", properties);
            TelemetryClient.TrackDialogView(Id);

            // Run first step
            return await RunStepAsync(dc, 0, DialogReason.BeginCalled, null, cancellationToken).ConfigureAwait(false);
        }

        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            // Don't do anything for non-message activities.
            if (dc.Context.Activity.Type != ActivityTypes.Message)
            {
                return Dialog.EndOfTurn;
            }

            // Run next step with the message text as the result.
            return await ResumeDialogAsync(dc, DialogReason.ContinueCalled, dc.Context.Activity.Text, cancellationToken).ConfigureAwait(false);
        }

        public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            // Increment step index and run step
            var state = dc.ActiveDialog.State;

            // For issue https://github.com/Microsoft/botbuilder-dotnet/issues/871
            // See the linked issue for details. This issue was happening when using the CosmosDB
            // data store for state. The stepIndex which was an object being cast to an Int64
            // after deserialization was throwing an exception for not being Int32 datatype.
            // This change ensures the correct datatype conversion has been done.
            var index = Convert.ToInt32(state[StepIndex]);
            return await RunStepAsync(dc, index + 1, reason, result, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Called when the dialog is ending.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of the conversation.</param>
        /// <param name="instance">The instance of the current dialog.</param>
        /// <param name="reason">The reason the dialog is ending.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public override Task EndDialogAsync(ITurnContext turnContext, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (reason == DialogReason.CancelCalled)
            {
                var state = new Dictionary<string, object>((Dictionary<string, object>)instance.State);

                // Create step context
                var index = Convert.ToInt32(state[StepIndex]);
                var stepName = WaterfallStepName(index);
                var instanceId = state[PersistedInstanceId] as string;

                var properties = new Dictionary<string, string>()
                {
                    { "DialogId", Id },
                    { "StepName", stepName },
                    { "InstanceId", instanceId },
                };
                TelemetryClient.TrackEvent("WaterfallCancel", properties);
            }
            else if (reason == DialogReason.EndCalled)
            {
                var state = new Dictionary<string, object>((Dictionary<string, object>)instance.State);
                var instanceId = state[PersistedInstanceId] as string;
                var properties = new Dictionary<string, string>()
                {
                    { "DialogId", Id },
                    { "InstanceId", instanceId },
                };
                TelemetryClient.TrackEvent("WaterfallComplete", properties);
            }

            return Task.CompletedTask;
        }

        protected virtual async Task<DialogTurnResult> OnStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var stepName = WaterfallStepName(stepContext.Index);
            var instanceId = stepContext.ActiveDialog.State[PersistedInstanceId] as string;
            var properties = new Dictionary<string, string>()
            {
                { "DialogId", Id },
                { "StepName", stepName },
                { "InstanceId", instanceId },
            };
            TelemetryClient.TrackEvent("WaterfallStep", properties);
            return await _steps[stepContext.Index](stepContext, cancellationToken).ConfigureAwait(false);
        }

        protected async Task<DialogTurnResult> RunStepAsync(DialogContext dc, int index, DialogReason reason, object result, CancellationToken cancellationToken)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            if (index < _steps.Count)
            {
                // Update persisted step index
                var state = dc.ActiveDialog.State;
                state[StepIndex] = index;

                // Create step context
                var options = state[PersistedOptions];
                var values = (IDictionary<string, object>)state[PersistedValues];
                var stepContext = new WaterfallStepContext(this, dc, options, values, index, reason, result);

                // Execute step
                return await OnStepAsync(stepContext, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // End of waterfall so just return any result to parent
                return await dc.EndDialogAsync(result).ConfigureAwait(false);
            }
        }

        private string WaterfallStepName(int index)
        {
            // Log Waterfall Step event. Each event has a distinct name to hook up
            // to the Application Insights funnel.
            var stepName = _steps[index].Method.Name;

            // Default stepname for lambdas
            if (string.IsNullOrWhiteSpace(stepName) || stepName.Contains("<"))
            {
                stepName = $"Step{index + 1}of{_steps.Count}";
            }

            return stepName;
        }
    }
}
