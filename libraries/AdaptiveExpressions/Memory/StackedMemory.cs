// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using AdaptiveExpressions.Properties;

namespace AdaptiveExpressions.Memory
{
    /// <summary>
    /// Stack implements of <see cref="IMemory"/>.
    /// Memory variables have a hierarchical relationship.
    /// </summary>
    public class StackedMemory : Stack<IMemory>, IMemory
    {
        public static StackedMemory Wrap(IMemory memory)
        {
            if (memory is StackedMemory sm)
            {
                return sm;
            }
            else
            {
                var stackedMemory = new StackedMemory();
                stackedMemory.Push(memory);
                return stackedMemory;
            }
        }

        /// <summary>
        /// Try get value from a given path.
        /// </summary>
        /// <param name="path">Given path.</param>
        /// <param name="value">Resolved value.</param>
        /// <returns>True if the memory contains an element with the specified key; otherwise, false.</returns>
        public bool TryGetValue(string path, out object value)
        {
            value = null;
            if (this.Count == 0)
            {
                return true;
            }

            var it = this.GetEnumerator();
            while (it.MoveNext())
            {
                var memory = it.Current;

                if (memory.TryGetValue(path, out var result) && result != null)
                {
                    value = result;
                    
                    if (value is IExpressionProperty ep)
                    {
                        value = ep.GetObject(memory);
                    }

                    return true;
                }
            }

            return true;
        }

        /// <summary>
        /// Set value to a given path.
        /// </summary>
        /// <param name="path">Memory path.</param>
        /// <param name="value">Value to set.</param>
        public void SetValue(string path, object value)
        {
            throw new Exception($"Can't set value to {path}, stacked memory is read-only");
        }

        public string Version()
        {
            return "0"; // Read-only
        }
    }
}
