// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    public class ChoiceInputOptions : InputDialogOptions
    {
        public List<Choice> Choices { get; set; }
    }
}
