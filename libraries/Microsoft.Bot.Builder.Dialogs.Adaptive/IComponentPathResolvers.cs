using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Dialogs.Memory;
using Microsoft.Bot.Builder.Dialogs.Memory.Scopes;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Declarative
{
    /// <summary>
    /// Interface for declaring path resolvers in the memory system.
    /// </summary>
    public interface IComponentPathResolvers
    {
        /// <summary>
        /// Return enumeration of pathresolvers.
        /// </summary>
        /// <returns>collection of IPathResolver.</returns>
        IEnumerable<IPathResolver> GetPathResolvers();
    }
}
