// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    /// <summary>
    /// Declarative input to gather float numbers from users
    /// </summary>
    public class FloatInput : NumberInput<float>
    {
        protected override string OnComputeId()
        {
            return $"FloatInput[{BindingPath()}]";
        }
    }
}
