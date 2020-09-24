// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// A date-time value, as recognized by the <see cref="DateTimePrompt"/>.
    /// </summary>
    /// <remarks>A value can represent a date, a time, a date and time, or a range of any of these.
    /// The representation of the value is determined by the locale used to parse the input.
    /// </remarks>
    public class DateTimeResolution
    {
        /// <summary>
        /// Gets or sets a human-readable represntation of the value, for a non-range result.
        /// </summary>
        /// <value>A human-readable represntation of the value, for a non-range result.</value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets a human-readable represntation of the start value, for a range result.
        /// </summary>
        /// <value>
        /// A human-readable represntation of the start value, for a range result.</value>
        public string Start { get; set; }

        /// <summary>
        /// Gets or sets a human-readable represntation of the end value, for a range result.
        /// </summary>
        /// <value>
        /// A human-readable represntation of the end value, for a range result.</value>
        public string End { get; set; }

        /// <summary>
        /// Gets or sets the value in TIMEX format.
        /// </summary>
        /// <value>A TIMEX representation of the value.</value>
        /// <remarks>The TIMEX format that follows the ISO 8601 standard.</remarks>
        public string Timex { get; set; }
    }
}
