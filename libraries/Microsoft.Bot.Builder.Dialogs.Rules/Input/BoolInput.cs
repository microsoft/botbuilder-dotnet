using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Input
{
    public class BoolInput : InputWrapper<ConfirmPrompt, bool>
    {
        protected override string OnComputeId()
        {
            return $"BoolInput[{BindingPath()}]";
        }
    }
}
