// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder
{
    public class SlotHistoryPolicy
    {
        public int MaxCount { get; set; }

        public int ExpiresAfterSeconds { get; set; }
    }
}
