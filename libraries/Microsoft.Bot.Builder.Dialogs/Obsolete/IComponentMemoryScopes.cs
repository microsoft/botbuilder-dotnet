// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Memory.Scopes;

namespace Microsoft.Bot.Builder.Dialogs.Memory
{
    /// <summary>
    /// Defines Component Memory Scopes interface for enumerating memory scopes.
    /// </summary>
    [Obsolete("Bot components should create subclass `Microsoft.Bot.Builder.BotComponent` and use the provided " +
    "`IServiceCollection` to register a custom memory scope. " +
    "Example: `services.AddSingleton<MemoryScope, MyCustomMemoryScope>()`. " +
    "In composer scenarios, the Startup method will be called automatically.")]
    public interface IComponentMemoryScopes
    {
        /// <summary>
        /// Gets the memory scopes.
        /// </summary>
        /// <returns>A <see cref="IEnumerable{MemoryScope}"/> with the memory scopes.</returns>
        IEnumerable<MemoryScope> GetMemoryScopes();
    }
}
