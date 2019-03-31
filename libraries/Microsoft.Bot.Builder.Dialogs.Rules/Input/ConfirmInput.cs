using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Input
{
    /// <summary>
    /// declarative input control that will gather yes/no confirmation input.
    /// </summary>
    public class ConfirmInput : InputWrapper<ConfirmPrompt, bool>
    {
        protected override string OnComputeId()
        {
            return $"ConfirmInput[{BindingPath()}]";
        }
    }
}
