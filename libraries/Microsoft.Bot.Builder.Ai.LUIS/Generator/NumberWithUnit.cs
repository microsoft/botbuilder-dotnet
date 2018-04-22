// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.LUIS
{
    /// <summary>
    /// Strongly typed class for LUIS number and unit entity recognition.
    /// </summary>
    /// <remarks>
    /// Specific subtypes of this class are generated to match the builtin age, currency, dimension and temperature entities.
    /// </remarks>
    public class NumberWithUnit
    {
        /// <summary>
        /// Recognized number, or null if unit only.
        /// </summary>
        [JsonProperty("number")]
        public readonly double? Number;

        /// <summary>
        /// Normalized recognized unit.
        /// </summary>
        [JsonProperty("units")]
        public readonly string Units;

        public NumberWithUnit(double? number, string units)
        {
            Number = number;
            Units = units;
        }
    }

    /// <summary>
    /// Strongly typed LUIS builtin_age.
    /// </summary>
    public class Age: NumberWithUnit
    {
        public Age(double number, string units) : base(number, units) { }

        public override string ToString() => $"Age({Number} {Units})";
    }

    /// <summary>
    /// Strongly typed LUIS builtin_dimension.
    /// </summary>
    public class Dimension: NumberWithUnit
    {
        public Dimension(double number, string units) : base(number, units) { }
        public override string ToString() => $"Dimension({Number} {Units})";
    }

    /// <summary>
    /// Strongly typed LUIS builtin_money.
    /// </summary>
    public class Money : NumberWithUnit
    {
        public Money(double number, string units) : base(number, units) { }
        public override string ToString() => $"Currency({Number} {Units})";
    }

    /// <summary>
    /// Strongly typed LUIS builtin_temperature.
    /// </summary>
    public class Temperature : NumberWithUnit
    {
        public Temperature(double number, string units) : base(number, units) { }
        public override string ToString() => $"Temperature({Number} {Units})";
    }
}
