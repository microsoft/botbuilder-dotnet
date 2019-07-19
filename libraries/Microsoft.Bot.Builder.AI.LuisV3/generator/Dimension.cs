// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.AI.Luis
{
    /// <summary>
    /// Strongly typed LUIS builtin_dimension.
    /// </summary>
    public class Dimension : NumberWithUnits
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Dimension"/> class.
        /// </summary>
        /// <param name="number">Number.</param>
        /// <param name="units">Units for number.</param>
        public Dimension(double number, string units)
            : base(number, units)
        {
        }

        /// <inheritdoc/>
        public override string ToString() => $"Dimension({Number} {Units})";
    }
}
