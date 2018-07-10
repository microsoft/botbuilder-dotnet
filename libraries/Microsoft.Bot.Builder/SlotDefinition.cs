// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder
{
    public class SlotDefinition
    {
        public string Name { get; set; }

        public object DefaultValue { get; set; }

        public string[] ChangeTags { get; set; }

        public int ExpiresAfterSeconds { get; set; }

        public SlotHistoryPolicy History { get; set; }
    }
}
