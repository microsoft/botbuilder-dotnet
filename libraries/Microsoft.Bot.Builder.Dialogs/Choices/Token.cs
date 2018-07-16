// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Choices
{
    public class Token
    {
        public int Start { get; set; }

        public int End { get; set; }

        public string Text { get; set; }

        public string Normalized { get; set; }
    }
}
