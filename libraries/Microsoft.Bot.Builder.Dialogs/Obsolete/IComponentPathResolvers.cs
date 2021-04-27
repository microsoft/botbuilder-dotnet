// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Memory
{
    /// <summary>
    /// Interface for declaring path resolvers in the memory system.
    /// </summary>
    [Obsolete("Bot components should create subclass `Microsoft.Bot.Builder.BotComponent` and use the provided " +
        "`IServiceCollection` to register a path resolver. " +
        "Example: `services.AddSingleton<IPathResolver, MyPathResolver>()`. " +
        "In composer scenarios, the Startup method will be called automatically.")]
    public interface IComponentPathResolvers
    {
        /// <summary>
        /// Return enumeration of pathresolvers.
        /// </summary>
        /// <returns>collection of IPathResolvers.</returns>
        IEnumerable<IPathResolver> GetPathResolvers();
    }
}
