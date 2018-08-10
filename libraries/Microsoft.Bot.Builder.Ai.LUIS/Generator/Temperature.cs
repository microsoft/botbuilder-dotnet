// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.Luis
{
    /// <summary>
    /// Strongly typed LUIS builtin_temperature.
    /// </summary>
    public class Temperature : NumberWithUnits
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Temperature"/> class.
        /// </summary>
        /// <param name="temperature">Temperature.</param>
        /// <param name="units">Units.</param>
        public Temperature(double temperature, string units)
            : base(temperature, units)
        {
        }

        /// <inheritdoc/>
        public override string ToString() => $"Temperature({Number} {Units})";
    }
}
