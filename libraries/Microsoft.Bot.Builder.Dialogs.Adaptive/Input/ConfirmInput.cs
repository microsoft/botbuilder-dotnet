// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    /// <summary>
    /// Declarative input control that will gather yes/no confirmation input.
    /// </summary>
    public class ConfirmInput : InputWrapper<ConfirmPrompt, bool>
    {
        protected override string OnComputeId()
        {
            return $"ConfirmInput[{BindingPath()}]";
        }
    }
}
