// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class PromptValidatorContext<T>
    {
        private readonly DialogContext _dc;

        internal PromptValidatorContext(DialogContext dc, IDictionary<string, object> state,  PromptOptions options, PromptRecognizerResult<T> recognized)
        {
            _dc = dc;
            HasEnded = false;
            EndResult = null;
            Options = options;
            Recognized = recognized;
        }

        public PromptOptions Options { get; }

        public PromptRecognizerResult<T> Recognized { get; }

        public IDictionary<string, object> State { get; }

        internal bool HasEnded { get; private set; }

        internal object EndResult { get; private set; }

        public void End(object result)
        {
            if (HasEnded)
            {
                throw new Exception($"PromptValidatorContext.End(): method already called for the turn.");
            }

            HasEnded = true;
            EndResult = result;
        }
    }
}
