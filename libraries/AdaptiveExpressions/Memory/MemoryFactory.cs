using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace AdaptiveExpressions.Memory
{
    public static class MemoryFactory
    {
        /// <summary>
        /// Get an appropriate IMemory implementation for an object.
        /// </summary>
        /// <param name="obj">Common object.</param>
        /// <returns>IMemory.</returns>
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
