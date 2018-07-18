// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.LUIS
{
    /// <summary>
    /// Strongly typed LUIS builtin_age.
    /// </summary>
    public class Age : NumberWithUnits
    {
        public Age(double number, string units)
            : base(number, units)
        {
        }

        public override string ToString() => $"Age({Number} {Units})";
    }
}
