// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    public class DelayActivity : Activity
    {
        public DelayActivity()
            : base(ActivityTypesEx.Delay)
        {
        }

        public TimeSpan Delay { get; set; }
    }
}
