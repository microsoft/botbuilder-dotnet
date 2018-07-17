// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Builder.Ai.LUIS
{
    /// <summary>
    /// This class represents built-in LUIS entities.
    /// </summary>
    public static partial class BuiltIn
    {
        /// <summary>
        /// Strongly-typed LUIS built-in date-time type.
        /// </summary>
        public static partial class DateTime
        {
            /// <summary>
            /// Identifies recognized times of day.
            /// </summary>
            public enum DayPart
            {
                /// <summary>
                /// Represents the morning.
                /// </summary>
                [Description("morning")]
                MO,

                /// <summary>
                /// Represents mid-day.
                /// </summary>
                [Description("midday")]
                MI,

                /// <summary>
                /// Represents the afternoon.
                /// </summary>
                [Description("afternoon")]
                AF,

                /// <summary>
                /// Represents the evening.
                /// </summary>
                [Description("evening")]
                EV,

                /// <summary>
                /// Represents the night.
                /// </summary>
                [Description("night")]
                NI,
            }

            /// <summary>
            /// Identifies relationships between times.
            /// </summary>
            public enum Reference
            {
                /// <summary>
                /// Represents the past.
                /// </summary>
                [Description("past")]
                PAST_REF,

                /// <summary>
                /// Represents the present.
                /// </summary>
                [Description("present")]
                PRESENT_REF,

                /// <summary>
                /// Represents the future.
                /// </summary>
                [Description("future")]
                FUTURE_REF,
            }

            /// <summary>
            /// Represents a LUIS date-time value.
            /// </summary>
            [Serializable]
            public sealed class DateTimeResolution : Resolution, IEquatable<DateTimeResolution>
            {
                /// <summary>
                /// The options LUIS uses when recognizing a date-time.
                /// </summary>
                public const RegexOptions Options = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace;

                /// <summary>
                /// The pattern LUIS uses to recognize a date.
                /// </summary>
                public const string PatternDate =
               @"
                    (?:
                        (?<year>X+|\d+)
                        (?:
                            -
                            (?<weekM>W)?
                            (?<month>X+|\d+)
                            (?:
                                -
                                (?<weekD>W)?
                                (?<day>X+|\d+)
                            )?
                        )?
                    )
                ";

                /// <summary>
                /// The pattern LUIS uses to recognize a time.
                /// </summary>
                public const string PatternTime =
                @"
                    (?:
                        T
                        (?:
                            (?<part>MO|MI|AF|EV|NI)
                        |
                            (?<hour>X+|\d+)
                            (?:
                                :
                                (?<minute>X+|\d+)
                                (?:
                                    :
                                    (?<second>X+|\d+)
                                )?
                            )?
                        )
                    )
                ";

                /// <summary>
                /// The pattern LUIS uses to recognize a date-time.
                /// </summary>
                public static readonly string Pattern = $"^({PatternDate}{PatternTime} | {PatternDate} | {PatternTime})$";

                /// <summary>
                /// The expression LUIS uses to recognize a date-time.
                /// </summary>
                public static readonly Regex Regex = new Regex(Pattern, Options);

                private static readonly IReadOnlyDictionary<string, Reference> ReferenceByText
                    = new Dictionary<string, Reference>(StringComparer.OrdinalIgnoreCase)
                    {
                                        { "PAST_REF", DateTime.Reference.PAST_REF },
                                        { "PRESENT_REF", DateTime.Reference.PRESENT_REF },
                                        { "FUTURE_REF", DateTime.Reference.FUTURE_REF },
                    };

                /// <summary>
                /// Initializes a new instance of the <see cref="DateTimeResolution"/> class.
                /// </summary>
                /// <param name="reference">Relationship to the time at which the result was generated.</param>
                /// <param name="year">The year.</param>
                /// <param name="month">The month.</param>
                /// <param name="day">The day of the month.</param>
                /// <param name="week">The week of the year.</param>
                /// <param name="dayOfWeek">The day of the week.</param>
                /// <param name="dayPart">The time of day.</param>
                /// <param name="hour">The hour.</param>
                /// <param name="minute">The minute.</param>
                /// <param name="second">The second.</param>
                public DateTimeResolution(
                   Reference? reference = null,
                   int? year = null,
                   int? month = null,
                   int? day = null,
                   int? week = null,
                   DayOfWeek? dayOfWeek = null,
                   DayPart? dayPart = null,
                   int? hour = null,
                   int? minute = null,
                   int? second = null)
                {
                    this.Reference = reference;
                    this.Year = year;
                    this.Month = month;
                    this.Day = day;
                    this.Week = week;
                    this.DayOfWeek = dayOfWeek;
                    this.DayPart = dayPart;
                    this.Hour = hour;
                    this.Minute = minute;
                    this.Second = second;
                }

                /// <summary>
                /// Initializes a new instance of the <see cref="DateTimeResolution"/> class.
                /// </summary>
                /// <param name="dateTime">The date-time to convert to a <see cref="DateTimeResolution"/>.</param>
                public DateTimeResolution(System.DateTime dateTime)
                {
                    this.Year = dateTime.Year;
                    this.Month = dateTime.Month;
                    this.Day = dateTime.Day;
                    this.Hour = dateTime.Hour;
                    this.Minute = dateTime.Minute;
                    this.Second = dateTime.Second;
                }

                /// <summary>
                /// Gets the relationship to the time at which the result was generated.
                /// </summary>
                /// <value>The relationship to the time at which the result was generated.</value>
                public Reference? Reference { get; }

                /// <summary>
                /// Gets the year.
                /// </summary>
                /// <value>The year.</value>
                public int? Year { get; }

                /// <summary>
                /// Gets the month.
                /// </summary>
                /// <value>The month.</value>
                public int? Month { get; }

                /// <summary>
                /// Gets the day of the month.
                /// </summary>
                /// <value>The day of the month.</value>
                public int? Day { get; }

                /// <summary>
                /// Gets the week of the year.
                /// </summary>
                /// <value>The week of the year.</value>
                public int? Week { get; }

                /// <summary>
                /// Gets the day of the week.
                /// </summary>
                /// <value>The day of the week.</value>
                public DayOfWeek? DayOfWeek { get; }

                /// <summary>
                /// Gets the time of day.
                /// </summary>
                /// <value>The time of day.</value>
                public DayPart? DayPart { get; }

                /// <summary>
                /// Gets the hour.
                /// </summary>
                /// <value>The hour.</value>
                public int? Hour { get; }

                /// <summary>
                /// Gets the minute.
                /// </summary>
                /// <value>The minute.</value>
                public int? Minute { get; }

                /// <summary>
                /// Gets the second.
                /// </summary>
                /// <value>The second.</value>
                public int? Second { get; }

                /// <summary>
                /// Converts a string into a LUIS date-time value.  A return value indicates whether the conversion succeeded.
                /// </summary>
                /// <param name="text">The string to parse.</param>
                /// <param name="resolution">When this method returns, contains the date-time, if the conversion succeeded, or
                /// null, if the conversion failed.</param>
                /// <returns>True if the conversion succeeded; otherwise, false.</returns>
                public static bool TryParse(string text, out DateTimeResolution resolution)
                {
                    if (ReferenceByText.TryGetValue(text, out var reference))
                    {
                        resolution = new DateTimeResolution(reference);
                        return true;
                    }

                    var match = Regex.Match(text);
                    if (match.Success)
                    {
                        var groups = match.Groups;
                        var weekM = groups["weekM"].Success;
                        var weekD = weekM || groups["weekD"].Success;

                        resolution = new DateTimeResolution(
                                year: ParseIntOrNull(groups["year"]),
                                week: weekM ? ParseIntOrNull(groups["month"]) : null,
                                month: !weekM ? ParseIntOrNull(groups["month"]) : null,
                                dayOfWeek: weekD ? (DayOfWeek?)ParseIntOrNull(groups["day"]) : null,
                                day: !weekD ? ParseIntOrNull(groups["day"]) : null,
                                dayPart: ParseEnumOrNull<DayPart>(groups["part"]),
                                hour: ParseIntOrNull(groups["hour"]),
                                minute: ParseIntOrNull(groups["minute"]),
                                second: ParseIntOrNull(groups["second"]));

                        return true;
                    }

                    resolution = null;
                    return false;
                }

                /// <inheritdoc/>
                public bool Equals(DateTimeResolution other) => other != null
                        && this.Reference == other.Reference
                        && this.Year == other.Year
                        && this.Month == other.Month
                        && this.Day == other.Day
                        && this.Week == other.Week
                        && this.DayOfWeek == other.DayOfWeek
                        && this.DayPart == other.DayPart
                        && this.Hour == other.Hour
                        && this.Minute == other.Minute
                        && this.Second == other.Second;

                /// <summary>
                /// Returns a value indicating whether this instance is equal to a specified object.
                /// </summary>
                /// <param name="other">The object to compare.</param>
                /// <returns>True if <paramref name="other"/> is a <see cref="DateTimeResolution"/> object
                /// and equals the value of this instance; otherwise, false.</returns>
                public override bool Equals(object other) => this.Equals(other as DateTimeResolution);

                /// <inheritdoc/>
                public override int GetHashCode() => throw new NotImplementedException();
            }

            private static int? ParseIntOrNull(Group group)
            {
                if (group.Success)
                {
                    var text = group.Value;
                    if (int.TryParse(text, out var number))
                    {
                        return number;
                    }
                    else if (text.Length > 0)
                    {
                        for (var index = 0; index < text.Length; ++index)
                        {
                            switch (text[index])
                            {
                                case 'X':
                                case 'x':
                                    continue;
                                default:
                                    throw new NotImplementedException();
                            }
                        }

                        // -1 means some variable X rather than missing "null" or specified constant value
                        return -1;
                    }
                }

                return null;
            }

            /// <summary>
            /// Converts the results from a single capturing group to an enumeration value.
            /// </summary>
            /// <typeparam name="T">The enumeration type.</typeparam>
            /// <param name="group">The capturing group.</param>
            /// <returns>The enumeration value, if successful; otherwise, null.</returns>
            private static T? ParseEnumOrNull<T>(Group group)
                where T : struct
            {
                if (group.Success)
                {
                    var text = group.Value;
                    if (Enum.TryParse<T>(text, out var result))
                    {
                        return result;
                    }
                }

                return null;
            }

            public sealed class DurationResolution
            {
            }
        }
    }
}
