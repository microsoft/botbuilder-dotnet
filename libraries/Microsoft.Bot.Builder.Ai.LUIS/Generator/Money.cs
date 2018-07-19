// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.LUIS
{
    /// <summary>
    /// Strongly typed LUIS builtin_money.
    /// </summary>
    public class Money : NumberWithUnits
    {
        public Money(double number, string units)
            : base(number, units)
        {
        }

        public override string ToString() => $"Currency({Number} {Units})";
    }
}
