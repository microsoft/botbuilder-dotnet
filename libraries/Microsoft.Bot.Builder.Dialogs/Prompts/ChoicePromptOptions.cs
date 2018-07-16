// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Choices;
using static Microsoft.Bot.Builder.Dialogs.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class ChoicePromptOptions : PromptOptions
    {
        public List<Choice> Choices
        {
            get { return GetProperty<List<Choice>>(nameof(Choices)); }
            set { this[nameof(Choices)] = value; }
        }
    }
}
