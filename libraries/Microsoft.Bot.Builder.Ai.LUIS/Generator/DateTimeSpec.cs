// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.LUIS
{
    /// <summary>
    /// Type for LUIS builtin_datetime.
    /// </summary>
    /// <remarks>
    /// LUIS recognizes time expressions like "next monday" and converts those to a type and set of timex expressions.
    /// More information on timex can be found here: http://www.timeml.org/publications/timeMLdocs/timeml_1.2.1.html#timex3.
    /// More information on the library which does the recognition can be found here: https://github.com/Microsoft/Recognizers-Text.
    /// </remarks>
    public class DateTimeSpec
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeSpec"/> class.
        /// </summary>
        /// <param name="type">Type of time expression, <see cref="Type"/>.</param>
        /// <param name="expressions">Sequence of timex expressions <see cref="DateTimeSpec"/>.</param>
        public DateTimeSpec(string type, IEnumerable<string> expressions)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (expressions == null)
            {
                throw new ArgumentNullException(nameof(expressions));
            }

            Type = type;
            Expressions = expressions.ToList();
        }

        /// <summary>
        /// Gets type of expression.
        /// </summary>
        /// <remarks>Example types include:
        /// <list type="*">
        /// <item>time -- simple time expression like "3pm".</item>
        /// <item>date -- simple date like "july 3rd".</item>
        /// <item>datetime -- combination of date and time like "march 23 2pm".</item>
        /// <item>timerange -- a range of time like "2pm to 4pm".</item>
        /// <item>daterange -- a range of dates like "march 23rd to 24th".</item>
        /// <item>datetimerang -- a range of dates and times like "july 3rd 2pm to 5th 4pm".</item>
        /// <item>set -- a recurrence like "every monday".</item>
        /// </list>
        /// </remarks>
        [JsonProperty("type")]
        public string Type { get; }

        /// <summary>
        /// Gets Timex expressions.
        /// </summary>
        [JsonProperty("timex")]
        public IReadOnlyList<string> Expressions { get; };

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"DateTimeSpec({Type}, [{string.Join(", ", Expressions)}]";
        }
    }
}
