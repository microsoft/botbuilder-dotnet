// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class PromptValidatorContext<T>
    {
        internal PromptValidatorContext(ITurnContext turnContext, PromptRecognizerResult<T> recognized, IDictionary<string, object> state, PromptOptions options)
        {
            Context = turnContext;
            Options = options;
            Recognized = recognized;
            State = state;
        }

        public ITurnContext Context { get; }

        public PromptRecognizerResult<T> Recognized { get; }

        public PromptOptions Options { get; }

        public IDictionary<string, object> State { get; }

        /// <summary>
        /// Gets the number of times the prompt has been executed.
        /// </summary>
        /// <value>A number indicating how many times the prompt was invoked (starting at 1 for the first time it was called).</value>
        public int AttemptCount => (int)State[Prompt<T>.AttemptCountKey];
    }
}
