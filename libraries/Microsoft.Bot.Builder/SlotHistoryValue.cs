// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder
{
    public class SlotHistoryValue<T>
    {
        public T Value { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
