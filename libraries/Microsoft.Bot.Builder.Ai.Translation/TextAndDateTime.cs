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
    /// TextAndDateTime Class used to store  text and date time object
    /// from Microsoft Recognizer recognition result.
    /// </summary>
    internal class TextAndDateTime
    {
        public string Text { get; set; }

        public DateTime DateTime { get; set; }

        public string Type { get; set; }

        public bool Range { get; set; }

        public DateTime EndDateTime { get; set; }
    }
}
