// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    public class EventActivityWithoutRetryPrompt : ActivityPrompt
    {
        private const string PersistedOptions = "options";
        private const string PersistedState = "state";

        public EventActivityWithoutRetryPrompt(string dialogId, PromptValidator<Activity> validator)
            : base(dialogId, validator)
        {
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            if (!(options is PromptOptions))
            {
                throw new ArgumentOutOfRangeException(nameof(options), "Prompt options are required for Prompt dialogs");
            }

            // Ensure prompts have input hint set
            var opt = (PromptOptions)options;
            if (opt.Prompt != null && string.IsNullOrEmpty(opt.Prompt.InputHint))
            {
                opt.Prompt.InputHint = InputHints.ExpectingInput;
            }

            if (opt.RetryPrompt != null && string.IsNullOrEmpty(opt.RetryPrompt.InputHint))
            {
                opt.RetryPrompt.InputHint = InputHints.ExpectingInput;
            }

            // Initialize prompt state
            var state = dc.ActiveDialog.State;
            state[PersistedOptions] = opt;
            state[PersistedState] = new Dictionary<string, object>
            {
                { "AttemptCount", 0 },
            };

            // Send initial prompt, calling OnPromptAsync() overload that does not have isRetry parameter
            await OnPromptAsync(dc.Context, (IDictionary<string, object>)state[PersistedState], (PromptOptions)state[PersistedOptions], cancellationToken).ConfigureAwait(false);
            return EndOfTurn;
        }
    }
}
