// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.Luis
{
    /// <summary>
    /// Strongly typed LUIS builtin_age.
    /// </summary>
    public class Age : NumberWithUnits
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Age"/> class.
        /// </summary>
        /// <param name="age">Age.</param>
        /// <param name="units">Units for age.</param>
        public Age(double age, string units)
            : base(age, units)
        {
        }

        /// <inheritdoc/>
        public override string ToString() => $"Age({Number} {Units})";
    }
}
