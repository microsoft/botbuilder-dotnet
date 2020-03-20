// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Memory.Scopes;

namespace Microsoft.Bot.Builder.Dialogs.Memory
{
    public interface IComponentMemoryScopes
    {
        IEnumerable<MemoryScope> GetMemoryScopes();
    }
}
