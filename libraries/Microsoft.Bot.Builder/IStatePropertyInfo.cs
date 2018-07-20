using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// This is metadata about the property including policy info.
    /// </summary>
    public interface IStatePropertyInfo
    {
        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <value>
        /// The name of the property.
        /// </value>
        string Name { get; }
    }
}
