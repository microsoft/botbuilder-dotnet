// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Memory
{
    /// <summary>
    /// Interface for declaring path resolvers in the memory system.
    /// </summary>
    public interface IComponentPathResolvers
    {
        /// <summary>
        /// Return enumeration of pathresolvers.
        /// </summary>
        /// <returns>collection of IPathResolvers.</returns>
        IEnumerable<IPathResolver> GetPathResolvers();
    }
}
