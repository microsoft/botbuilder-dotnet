using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Microsoft.Bot.AdaptiveExpressions.Core.Memory
{
    /// <summary>
    /// Memory Factory for creating an object that implements IMemory interface.
    /// </summary>
    public static class MemoryFactory
    {
        /// <summary>
        /// Get an appropriate IMemory implementation for an object.
        /// </summary>
        /// <param name="obj">Common object.</param>
        /// <returns>IMemory.</returns>
        [RequiresUnreferencedCode("MemoryFactory uses reflection, use overloads that take IMemory only")]
        [RequiresDynamicCode("MemoryFactory uses reflection, use overloads that take IMemory only")]
        public static IMemory Create(object obj)
        {
            if (obj != null)
            {
                if (obj is IMemory)
                {
                    return (IMemory)obj;
                }

                // if this is ducktype of IMemory
                var memory = ReflectionMemory.Create(obj);
                if (memory != null)
                {
                    return memory;
                }
            }

            return new SimpleObjectMemory(obj);
        }
    }
}
