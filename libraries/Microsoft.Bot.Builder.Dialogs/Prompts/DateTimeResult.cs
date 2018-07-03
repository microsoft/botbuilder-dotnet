// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// The result of a DateTime prompt
    /// Note there might be 1, 2 or 4 resolutions depending on the particular scenario.
    /// For example:
    /// - a specific date and time like "5th December 2018 at 9am" results in a single resolution
    /// - a date with some ambiguity like "4th October" results in a single TIMEX but still 2 example values and so 2 resolutions
    /// - a date and time with ambiguity like Octerber 4 4 Oclock" results in two TIMXE and 4 example values so 4 resolutions
    /// </summary>
    public class DateTimeResult : PromptResult
    {
        public DateTimeResult()
        {
            Resolution = new List<DateTimeResolution>();
        }

        /// <summary>
        /// The input text recognized; or <c>null</c>, if recognition fails.
        /// </summary>
        public string Text
        {
            get { return GetProperty<string>(nameof(Text)); }
            set { this[nameof(Text)] = value; }
        }

        /// <summary>
        /// The various resolutions for the recognized value; or and empty list.
        /// </summary>
        public List<DateTimeResolution> Resolution
        {
            get { return GetProperty<List<DateTimeResolution>>(nameof(Resolution)); }
            private set { this[nameof(Resolution)] = value; }
        }

        public class DateTimeResolution
        {
            public string Value { get; set; }
            public string Start { get; set; }
            public string End { get; set; }
            public string Timex { get; set; }
        }
    }
}