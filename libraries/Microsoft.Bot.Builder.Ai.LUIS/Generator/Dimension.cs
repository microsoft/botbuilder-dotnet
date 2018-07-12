// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.LUIS
{

    /// <summary>
    /// Strongly typed LUIS builtin_dimension.
    /// </summary>
    public class Dimension : NumberWithUnits
    {
        public Dimension(double number, string units)
            : base(number, units)
        {
        }

        public override string ToString() => $"Dimension({Number} {Units})";
    }
}
