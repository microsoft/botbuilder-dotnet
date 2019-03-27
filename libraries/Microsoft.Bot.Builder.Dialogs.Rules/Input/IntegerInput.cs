using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Input
{
    public class IntegerInput : NumberInput<int>
    {
        protected override string OnComputeId()
        {
            return $"IntegerInput[{BindingPath()}]";
        }
    }
}
