// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    /// <summary>
    /// Declarative text input to gather choices from users
    /// </summary>
    public class ChoiceInput : InputWrapper<ChoicePrompt, FoundChoice>
    {
        public List<Choice> Choices { get; set; }

        public string ChoicesProperty { get; set; }

        public ListStyle Style { get; set; }

        // Override the base method since we need to pass choices to the prompt options
        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            // Check value in state and only call if missing or required by AlwaysPrompt
            var hasValue = Property == null ? false : dc.State.HasValue<FoundChoice>(Property);

            if (hasValue == false || AlwaysPrompt)
            {
                if (Prompt == null)
                {
                    throw new ArgumentNullException(nameof(Activity));
                }

                var prompt = await Prompt.BindToData(dc.Context, dc.State).ConfigureAwait(false);
                var retryPrompt = RetryPrompt == null ? prompt : await RetryPrompt.BindToData(dc.Context, dc.State).ConfigureAwait(false);

                this.prompt.Style = this.Style;

                var choices = this.Choices ?? new List<Choice>();

                if (!string.IsNullOrEmpty(this.ChoicesProperty))
                {
                    var choiceValue = dc.State.GetValue<object>(this.ChoicesProperty);
                    if (choiceValue != null)
                    {
                        try
                        {
                            if (choiceValue is string)
                            {
                                choices = JsonConvert.DeserializeObject<List<Choice>>(choiceValue.ToString());
                            }
                            else if (choiceValue is List<Choice>)
                            {
                                choices = (List<Choice>)choiceValue;
                            }
                        }
                        catch
                        {

                        }
                    }
                }

                return await dc.PromptAsync(this.prompt.Id, new ChoicePromptOptions() { Prompt = prompt, RetryPrompt = retryPrompt, Choices = choices}, cancellationToken).ConfigureAwait(false);
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
