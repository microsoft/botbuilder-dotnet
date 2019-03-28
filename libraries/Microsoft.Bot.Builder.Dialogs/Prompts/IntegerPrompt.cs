// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Prompts;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.Number;
using static Microsoft.Recognizers.Text.Culture;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Prompt for an integer number (aka 10).
    /// </summary>
    public class IntegerPrompt : NumberPrompt<int>
    {
        public IntegerPrompt()
            : base()
        {
        }

        public IntegerPrompt(string dialogId = null, PromptValidator<int> validator = null, string defaultLocale = null)
            : base(dialogId, validator, defaultLocale)
        {
        }

        protected override string OnComputeId()
        {
            return $"IntegerPrompt[{this.BindingPath()}]";
        }
    }
}
