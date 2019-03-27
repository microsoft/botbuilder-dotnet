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
    /// Prompt for a float number (aka, 1.0 1.5)
    /// </summary>
    public class FloatPrompt : NumberPrompt<float>
    {
        public FloatPrompt()
           : base()
        {
        }

        public FloatPrompt(string dialogId = null, PromptValidator<float> validator = null, string defaultLocale = null)
            : base(dialogId, validator, defaultLocale)
        {
        }

        protected override string OnComputeId()
        {
            return $"FloatPrompt[{this.BindingPath()}]";
        }
    }
}
