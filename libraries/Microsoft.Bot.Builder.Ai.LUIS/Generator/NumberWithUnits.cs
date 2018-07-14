// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.Luis
{
    /// <summary>
    /// Strongly typed class for LUIS number and units entity recognition.
    /// </summary>
    /// <remarks>
    /// Specific subtypes of this class are generated to match the builtin age, currency, dimension and temperature entities.
    /// </remarks>
    public class NumberWithUnits
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NumberWithUnits"/> class.
        /// </summary>
        /// <param name="number">Number.</param>
        /// <param name="units">Units for number.</param>
        public NumberWithUnits(double? number, string units)
        {
            Number = number;
            Units = units;
        }

        /// <summary>
        /// Gets or sets recognized number, or null if unit only.
        /// </summary>
        [JsonProperty("number")]
        public double? Number { get; set; }

        /// <summary>
        /// Gets or sets normalized recognized unit.
        /// </summary>
        [JsonProperty("units")]
        public string Units { get; set; }
    }
}
