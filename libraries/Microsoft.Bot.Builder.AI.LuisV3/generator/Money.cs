// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.Luis
{
    /// <summary>
    /// Strongly typed LUIS builtin_money.
    /// </summary>
    public class Money : NumberWithUnits
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Money"/> class.
        /// </summary>
        /// <param name="money">Money amount.</param>
        /// <param name="units">Currency units.</param>
        public Money(double money, string units)
            : base(money, units)
        {
        }

        /// <inheritdoc/>
        public override string ToString() => $"Currency({Number} {Units})";
    }
}
