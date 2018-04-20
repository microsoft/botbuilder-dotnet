// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Builder.Ai.LUIS
{
    /// <summary>
    /// Type for LUIS builtin_datetime.
    /// </summary>
    /// <remarks>
    /// LUIS recognizes time expressions like "next monday" and converts those to a type and set of timex expressions.
    /// More information on timex can be found here: http://www.timeml.org/publications/timeMLdocs/timeml_1.2.1.html#timex3
    /// </remarks>
    public class DateTimeSpec
    {
        /// <summary>
        /// Type of expression.
        /// </summary>
        [JsonProperty("type")]
        public readonly string Type;

        /// <summary>
        /// Timex expressions.
        /// </summary>
        [JsonProperty("timex")]
        public readonly IList<string> Expressions;

        public DateTimeSpec(string type, IEnumerable<string> expressions)
        {
            Type = type;
            Expressions = expressions.ToList();
        }

        public override string ToString()
        {
            return $"DateTimeSpec({Type}, [{String.Join(", ", Expressions)}]";
        }
    }
}
