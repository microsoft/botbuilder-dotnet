// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Memory.Scopes;

namespace Microsoft.Bot.Builder.Dialogs.Memory
{
    /// <summary>
    /// Defines Component Memory Scopes interface for enumerating memory scopes.
    /// </summary>
    public interface IComponentMemoryScopes
    {
        /// <summary>
        /// Gets the memory scopes.
        /// </summary>
        /// <returns>A <see cref="IEnumerable{MemoryScope}"/> with the memory scopes.</returns>
        IEnumerable<MemoryScope> GetMemoryScopes();
    }
}
