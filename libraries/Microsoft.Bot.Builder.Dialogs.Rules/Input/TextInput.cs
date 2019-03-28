using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Input
{
    public class TextInput : InputWrapper<TextPrompt, string>
    {
        private Regex _patternMatcher;

        /// <summary>
        /// Regex Match expression to match.
        /// </summary>
        public string Pattern { get { return _patternMatcher?.ToString(); } set { _patternMatcher = new Regex(value); } }

        protected override TextPrompt CreatePrompt()
        {
            return new TextPrompt(null, new PromptValidator<string>(async (promptContext, cancel) =>
            {
                if (!promptContext.Recognized.Succeeded)
                {
                    return false;
                }

                if (_patternMatcher == null)
                {
                    return true;
                }

                var value = promptContext.Recognized.Value;

                if (!_patternMatcher.IsMatch(value))
                {
                    if (InvalidPrompt != null)
                    {
                        var invalid = await InvalidPrompt.BindToData(promptContext.Context, promptContext.State).ConfigureAwait(false);
                        if (invalid != null)
                        {
                            await promptContext.Context.SendActivityAsync(invalid).ConfigureAwait(false);
                        }

                    }

                    return false;
                }

                return true;
            }));
        }

        protected override string OnComputeId()
        {
            return $"TextInput[{BindingPath()}]";
        }
    }
}
