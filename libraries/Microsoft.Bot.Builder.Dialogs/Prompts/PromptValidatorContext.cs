using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class PromptValidatorContext<T>
    {
        private readonly DialogContext _dc;
        private bool _hasEnded;
        private object _endResult;

        internal PromptValidatorContext(DialogContext dc, IDictionary<string, object> state,  PromptOptions options, PromptRecognizerResult<T> recognized)
        {
            _dc = dc;
            _hasEnded = false;
            _endResult = null;
            Options = options;
            Recognized = recognized;
        }

        internal bool HasEnded { get => _hasEnded; }

        internal object EndResult { get => _endResult; }

        public PromptOptions Options { get; }

        public PromptRecognizerResult<T> Recognized { get; }

        public IDictionary<string, object> State { get; }

        public void End(object result)
        {
            if (_hasEnded)
            {
                throw new Exception($"PromptValidatorContext.End(): method already called for the turn.");
            }

            _hasEnded = true;
            _endResult = result;
        }
    }
}
