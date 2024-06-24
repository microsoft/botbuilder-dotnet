// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Bot.AdaptiveExpressions.Core.Properties;

namespace Microsoft.Bot.AdaptiveExpressions.Core.Memory
{
    /// <summary>
    /// Simple implement of <see cref="IMemory"/>.
    /// </summary>
    [RequiresDynamicCode("SimpleObjectMemory requires reflection and is not AOT compatible")]
    [RequiresUnreferencedCode("SimpleObjectMemory requires reflection and is not AOT compatible")]
    public class SimpleObjectMemory : IMemory
    {
        private object _memory = null;
        private JsonSerializerContext _serializerContext = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleObjectMemory"/> class.
        /// This wraps a simple object as IMemory.
        /// </summary>
        /// <param name="memory">The object to wrap.</param>
        /// <param name="serializerContext">optional JsonSerializerContext for serialization.</param>
        public SimpleObjectMemory(object memory, JsonSerializerContext serializerContext = null)
        {
            _memory = memory;
            _serializerContext = serializerContext;
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
            if (_memory == null || path.Length == 0)
            {
                return false;
            }

            var curScope = _memory;

            foreach (var part in new SimplePathEnumerator(path))
            {
                string error = null;
                if (part.Index is int idx && FunctionUtils.TryParseList(curScope, out var li))
                {
                    (value, error) = FunctionUtils.AccessIndex(li, idx);
                    if (error != null)
                    {
                        return false;
                    }
                }
                else
                {
                    if (!FunctionUtils.TryAccessProperty(curScope, part.Part, out value))
                    {
                        return false;
                    }
                }

                curScope = value;
            }

            if (value is IExpressionProperty ep)
            {
                value = ep.GetObject(MemoryFactory.Create(_memory));
            }

            return true;
        }

        // In this simple object scope, we don't allow you to set a path in which some parts in middle don't exist
        // for example
        // if you set dialog.a.b = x, but dialog.a don't exist, this will result in an error
        // because we can't and shouldn't smart create structure in the middle
        // you can implement a customized Scope that support such behavior

        /// <summary>
        /// Set value to a given path.
        /// </summary>
        /// <param name="path">Memory path.</param>
        /// <param name="value">Value to set.</param>
        public void SetValue(string path, object value)
        {
            if (_memory == null)
            {
                return;
            }

            var curScope = _memory;
            var curPath = string.Empty; // valid path so far
            string error = null;

            // find the 2nd last value, the container
            foreach (var part in new SimplePathEnumerator(path))
            {
                // For all parts until the list, evaluate as path
                if (!part.IsLast)
                {
                    if (part.Index is int index && FunctionUtils.TryParseList(curScope, out var li))
                    {
                        curPath += $"[{part.Part}]";
                        (curScope, error) = FunctionUtils.AccessIndex(li, index);
                    }
                    else
                    {
                        curPath += $".{part.Part}";
                        if (FunctionUtils.TryAccessProperty(curScope, part.Part, out var newScope))
                        {
                            curScope = newScope;
                        }
                        else
                        {
                            return;
                        }
                    }

                    if (error != null || curScope == null)
                    {
                        return;
                    }
                }
                else
                {
                    // set the last value
                    if (part.Index is int idx)
                    {
                        if (FunctionUtils.TryAsList(curScope, out var li))
                        {
                            var count = FunctionUtils.GetListCount(li);
                            if (idx > count)
                            {
                                error = $"{idx} index out of range";
                            }
                            else if (idx == count)
                            {
                                // expand for one
                                FunctionUtils.AppendToList(li, value);
                            }
                            else
                            {
                                FunctionUtils.SetIndex(li, idx, value);
                            }
                        }
                        else
                        {
                            error = $"set value for an index to a non-list object";
                        }

                        if (error != null)
                        {
                            return;
                        }
                    }
                    else
                    {
                        (_, error) = SetProperty(curScope, part.Part, value);
                        if (error != null)
                        {
                            return;
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public IMemory CreateMemoryFrom(object value)
        {
            return MemoryFactory.Create(value);
        }

        /// <summary>
        /// Return the version info of SimpleObjectMemory.
        /// </summary>
        /// <returns>A string value.</returns>
        public string Version()
        {
            return ToString();
        }

        /// <inheritdoc/>
        public string JsonSerializeToString(object value)
        {
            return JsonSerializer.Serialize(value, _serializerContext?.Options);
        }

        /// <inheritdoc/>
        public JsonNode SerializeToNode(object value)
        {
            return value == null ? null : JsonSerializer.SerializeToNode(value, _serializerContext?.Options);
        }

        /// <inheritdoc/>
        public object ConvertTo(Type type, object value)
        {
            return JsonSerializer.Deserialize(JsonSerializer.SerializeToNode(value, _serializerContext?.Options), type, _serializerContext?.Options);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string value.</returns>
        public override string ToString()
        {
            return JsonSerializer.Serialize(_memory, new JsonSerializerOptions(_serializerContext?.Options) { ReferenceHandler = ReferenceHandler.IgnoreCycles });
        }

        private (object result, string error) SetProperty(object instance, string property, object value)
        {
            var result = value;
            string error = null;

            if (instance is IDictionary<string, object> idict)
            {
                idict[property] = value;
            }
            else if (instance is IDictionary dict)
            {
                dict[property] = value;
            }
            else if (instance is JsonObject jobj)
            {
                jobj[property] = JsonSerializer.SerializeToNode(value, _serializerContext?.Options);
            }
            else
            {
                // Use reflection
                var type = instance.GetType();
                var prop = type.GetProperties().Where(p => p.Name.ToLowerInvariant() == property).SingleOrDefault();
                if (prop != null)
                {
                    if (prop.CanWrite)
                    {
                        prop.SetValue(instance, value);
                    }
                    else
                    {
                        error = $"property {prop.Name} is read-only";
                    }
                }
            }

            return (result, error);
        }
    }
}
