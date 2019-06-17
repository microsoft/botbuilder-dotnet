// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    /// <summary>
    /// Declarative text input to gather text data from users
    /// </summary>
    public class TextInput : InputDialog
    {
        private Regex _patternMatcher;

        /// <summary>
        /// Regex Match expression to match.
        /// </summary>
        public string Pattern { get { return _patternMatcher?.ToString(); } set { _patternMatcher = new Regex(value); } }


        public TextInput([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        //protected override TextPrompt CreatePrompt()
        //{
        //    return new TextPrompt(null, new PromptValidator<string>(async (promptContext, cancel) =>
        //    {
        //        if (!promptContext.Recognized.Succeeded)
        //        {
        //            return false;
        //        }

        //        if (_patternMatcher == null)
        //        {
        //            return true;
        //        }

        //        var value = promptContext.Recognized.Value;

        //        if (!_patternMatcher.IsMatch(value))
        //        {
        //            if (InvalidPrompt != null)
        //            {
        //                var invalid = await InvalidPrompt.BindToData(promptContext.Context, promptContext.State).ConfigureAwait(false);
        //                if (invalid != null)
        //                {
        //                    await promptContext.Context.SendActivityAsync(invalid).ConfigureAwait(false);
        //                }

        //            }

        //            return false;
        //        }

        //        return true;
        //    }));
        //}

        protected override string OnComputeId()
        {
            return $"TextInput[{BindingPath()}]";
        }

        protected override async Task<InputState> OnRecognizeInput(DialogContext dc, bool consultation)
        {
            if (consultation)
            {
                return InputState.Unrecognized;
            }

            var input = dc.State.GetValue<string>(INPUT_PROPERTY);

            return input.Length > 0 ? InputState.Valid : InputState.Unrecognized;

        }
    }
}
