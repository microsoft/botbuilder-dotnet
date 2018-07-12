// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.LUIS
{

    /// <summary>
    /// Strongly typed LUIS builtin_temperature.
    /// </summary>
    public class Temperature : NumberWithUnits
    {
        public Temperature(double number, string units)
            : base(number, units)
        {
        }

        public override string ToString() => $"Temperature({Number} {Units})";
    }
}
