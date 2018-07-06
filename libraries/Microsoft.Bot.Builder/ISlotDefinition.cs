// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.ObjectModel;

namespace Microsoft.Bot.Builder
{
    public interface ISlotDefinition
    {
        string Name { get; set; }

        ReadOnlyCollection<string> ChangeTags { get; }

        int ExpiresAfterSeconds { get; set; }

        SlotHistoryPolicy History { get; set; }
    }
}
