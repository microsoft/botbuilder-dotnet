// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Bot.AdaptiveExpressions.Core.BuiltinFunctions;
using Microsoft.Bot.AdaptiveExpressions.Core.Properties;

namespace Microsoft.Bot.AdaptiveExpressions.Core.Memory
{
    /// <summary>
    /// Stack implements of <see cref="IMemory"/>.
    /// Memory variables have a hierarchical relationship.
    /// </summary>
#pragma warning disable CA1710 // Identifiers should have correct suffix (we can't change this without breaking binary compat)
#pragma warning disable CA1010 // Generic interface should also be implemented (excluding for now, the designers of this package should evaluate complying in the future, for more info see https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1010?view=vs-2019)
    public class StackedMemory : Stack<IMemory>, IMemory
#pragma warning restore CA1010 // Generic interface should also be implemented
#pragma warning restore CA1710 // Identifiers should have correct suffix
    {
        /// <summary>
        /// Wrap an object implementes IMemory interface into a StackedMemory object.
        /// </summary>
        /// <param name="memory">An object that implements IMemory.</param>
        /// <returns>A StackedMemory object.</returns>
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

                if (memory.TryGetValue(path, out var result))
                {
                    value = result;
                    
                    if (value is IExpressionProperty ep)
                    {
                        value = ep.GetObject(memory);
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Set value to a given path.
        /// </summary>
        /// <param name="path">Memory path.</param>
        /// <param name="value">Value to set.</param>
        public void SetValue(string path, object value)
        {
            throw new InvalidOperationException($"Can't set value to {path}, stacked memory is read-only");
        }

        /// <inheritdoc/>
        public IMemory CreateMemoryFrom(object value)
        {
            return this.Last().CreateMemoryFrom(value);
        }

        /// <inheritdoc/>
        public string JsonSerializeToString(object value)
        {
            return this.Last().JsonSerializeToString(value);
        }

        /// <inheritdoc/>
        public JsonNode SerializeToNode(object value)
        {
            return this.Last().SerializeToNode(value);
        }

        /// <inheritdoc/>
        public object ConvertTo(Type type, object value)
        {
            return this.Last().ConvertTo(type, value);
        }

        /// <summary>
        /// Get the version of the current StackedMemory.
        /// </summary>
        /// <returns>A string value.</returns>
        public string Version()
        {
            return "0"; // Read-only
        }

        /// <summary>
        /// Push a frame with just a single variable in it.
        /// </summary>
        /// <param name="propertyName">name.</param>
        /// <param name="value">value.</param>
        public void PushLocalIterator(string propertyName, object value)
        {
            Push(new StackedMemoryFrame { PropertyName = propertyName, Value = value, Parent = this });
        }

        private class StackedMemoryFrame : IMemory
        {
            public string PropertyName { get; init; }

            public object Value { get; init; }

            public StackedMemory Parent { get; init; }

            public IMemory CreateMemoryFrom(object value)
            {
                return Parent.CreateMemoryFrom(value);
            }

            public string JsonSerializeToString(object value)
            {
                return Parent.JsonSerializeToString(value);
            }

            public JsonNode SerializeToNode(object value)
            {
                return Parent.SerializeToNode(value);
            }

            public object ConvertTo(Type type, object value)
            {
                return Parent.ConvertTo(type, value);
            }

            public void SetValue(string path, object value)
            {
                throw new NotImplementedException();
            }

            public bool TryGetValue(string path, out object value)
            {
                var pathSplit = path.Split(['.', '['], 2);
                if (pathSplit.Length == 1)
                {
                    // If the path matches the property name exactly, return it.
                    if (path == PropertyName)
                    {
                        value = Value;
                        return true;
                    }
                }
                else
                {
                    // If the path has anything left, delegate to IMemory to resolve the rest.
                    var (pathStart, pathRemainder) = (pathSplit[0], pathSplit[1]);
                    if (pathStart == PropertyName)
                    {
                        // If we split on [, add that back before we keep going
                        if (path[pathStart.Length] == '[')
                        {
                            pathRemainder = "[" + pathRemainder;
                        }

                        return Parent.CreateMemoryFrom(Value).TryGetValue(pathRemainder, out value);
                    }
                }

                value = null;
                return false;
            }

            public string Version()
            {
                return Parent.Version();
            }
        }
    }
}
