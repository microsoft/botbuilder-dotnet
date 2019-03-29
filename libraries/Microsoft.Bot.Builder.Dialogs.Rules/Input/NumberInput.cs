using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Input
{
    public class NumberInput<TNumber> : InputWrapper<NumberPrompt<TNumber>, TNumber> where TNumber : struct, IComparable<TNumber>
    {
        public TNumber MinValue { get; set; }
        public TNumber MaxValue { get; set; }

        public NumberInput()
        {
        }

        protected override NumberPrompt<TNumber> CreatePrompt()
        {
            return new NumberPrompt<TNumber>(null, new PromptValidator<TNumber>(async (promptContext, cancel) =>
            {
                var result = (IComparable<TNumber>)promptContext.Recognized.Value;

                if (result.CompareTo(MinValue) < 0 || result.CompareTo(MaxValue) > 0)
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
    }
}
