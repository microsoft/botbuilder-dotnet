// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;

namespace Microsoft.Bot.Builder.Ai.Translation
{

    /// <summary>
    /// DateAndTimeLocaleFormat Class used to store date format and time format
    /// for different locales.
    /// </summary>
    internal class DateAndTimeLocaleFormat
    {
        public string TimeFormat { get; set; }

        public string DateFormat { get; set; }
    }
}
