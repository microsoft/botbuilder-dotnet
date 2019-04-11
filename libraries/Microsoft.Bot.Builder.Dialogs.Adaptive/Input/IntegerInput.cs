// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    /// <summary>
    /// Declarative input class to gather integer data from users
    /// </summary>
    public class IntegerInput : NumberInput<int>
    {
        public IntegerInput()
        {
            this.MinValue = int.MinValue;
            this.MaxValue = int.MaxValue;
        }

        protected override string OnComputeId()
        {
            return $"IntegerInput[{BindingPath()}]";
        }
    }
}
