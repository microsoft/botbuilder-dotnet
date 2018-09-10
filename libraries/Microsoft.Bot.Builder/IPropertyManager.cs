using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// IPropertyManager defines implementation of a source of named properties.
    /// </summary>
    public interface IPropertyManager
    {
        /// <summary>
        /// Create a managed state property accessor for named property.
        /// </summary>
        /// <typeparam name="T">type of object</typeparam>
        /// <param name="name">name of the object</param>
        /// <returns>property accessor for accessing the object of type T.</returns>
        IStatePropertyAccessor<T> CreateProperty<T>(string name);
    }
}
