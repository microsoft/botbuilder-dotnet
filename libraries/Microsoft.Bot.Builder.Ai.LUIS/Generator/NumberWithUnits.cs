// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.LUIS
{
    /// <summary>
    /// Strongly typed class for LUIS number and units entity recognition.
    /// </summary>
    /// <remarks>
    /// Specific subtypes of this class are generated to match the builtin age, currency, dimension and temperature entities.
    /// </remarks>
    public class NumberWithUnits
    {
        public NumberWithUnits(double? number, string units)
        {
            Number = number;
            Units = units;
        }

        /// <summary>
        /// Gets the recognized number, or null if unit only.
        /// </summary>
        /// <value>
        /// Recognized number, or null if unit only.
        /// </value>
        [JsonProperty("number")]
        public double? Number { get; }

        /// <summary>
        /// Gets the normalized recognized unit.
        /// </summary>
        /// <value>
        /// Normalized recognized unit.
        /// </value>
        [JsonProperty("units")]
        public string Units { get; }
    }
}
