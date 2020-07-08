// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Memory.Scopes;

namespace Microsoft.Bot.Builder.Dialogs.Memory
{
    public interface IComponentMemoryScopes
    {
        IEnumerable<MemoryScope> GetMemoryScopes();
    }
}
