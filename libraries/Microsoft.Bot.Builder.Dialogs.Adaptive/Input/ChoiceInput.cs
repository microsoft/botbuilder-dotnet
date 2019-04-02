// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    /// <summary>
    /// Declarative text input to gather choices from users
    /// </summary>
    public class ChoiceInput : InputWrapper<ChoicePrompt, FoundChoice>
    {
        public List<Choice> choices { get; set; }

        protected override ChoicePrompt CreatePrompt()
        {
            return new ChoicePrompt()
            { };

        }
        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Check value in state and only call if missing or required by AlwaysPrompt
            var value = dc.State.GetValue<FoundChoice>(Property);

            if (value == null || AlwaysPrompt)
            {
                if (Prompt == null)
                {
                    throw new ArgumentNullException(nameof(Activity));
                }

                var prompt = await Prompt.BindToData(dc.Context, dc.State).ConfigureAwait(false);
                var retryPrompt = RetryPrompt == null ? prompt : await RetryPrompt.BindToData(dc.Context, dc.State).ConfigureAwait(false);

                return await dc.PromptAsync(nameof(ChoicePrompt), new ChoicePromptOptions() { Prompt = prompt, RetryPrompt = retryPrompt, Choices = this.choices}, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken);
            }
        }
        protected override string OnComputeId()
        {
            return $"ChoiceInput[{BindingPath()}]";
        }
    }
}
