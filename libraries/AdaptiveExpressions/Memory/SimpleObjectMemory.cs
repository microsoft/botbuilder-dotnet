// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AdaptiveExpressions.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions.Memory
{
    /// <summary>
    /// Simple implement of <see cref="IMemory"/>.
    /// </summary>
    public class SimpleObjectMemory : IMemory
    {
        private object _memory = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleObjectMemory"/> class.
        /// This wraps a simple object as IMemory.
        /// </summary>
        /// <param name="memory">The object to wrap.</param>
        public SimpleObjectMemory(object memory)
        {
            _memory = memory;
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

            var parts = path.Split(".[]".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Trim('\'', '"'))
                            .ToArray();

            var curScope = _memory;

            foreach (var part in parts)
            {
                string error = null;
                if (int.TryParse(part, out var idx) && FunctionUtils.TryParseList(curScope, out var li))
                {
                    (value, error) = FunctionUtils.AccessIndex(li, idx);
                    if (error != null)
                    {
                        return false;
                    }
                }
                else
                {
                    if (!FunctionUtils.TryAccessProperty(curScope, part, out value))
                    {
                        return false;
                    }
                }

                curScope = value;
            }

            if (value is IExpressionProperty ep)
            {
                value = ep.GetObject(_memory);
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

            var parts = path.Split(".[]".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Trim('\'', '"'))
                            .ToArray();

            var curScope = _memory;
            var curPath = string.Empty; // valid path so far
            string error = null;

            // find the 2nd last value, the container
            for (var i = 0; i < parts.Length - 1; i++)
            {
                if (int.TryParse(parts[i], out var index) && FunctionUtils.TryParseList(curScope, out var li))
                {
                    curPath += $"[{parts[i]}]";
                    (curScope, error) = FunctionUtils.AccessIndex(li, index);
                }
                else
                {
                    curPath += $".{parts[i]}";
                    if (FunctionUtils.TryAccessProperty(curScope, parts[i], out var newScope))
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

            // set the last value
            if (int.TryParse(parts.Last(), out var idx))
            {
                if (FunctionUtils.TryParseList(curScope, out var li))
                {
                    if (li is JArray)
                    {
                        value = JToken.FromObject(value);
                    }

                    if (idx > li.Count)
                    {
                        error = $"{idx} index out of range";
                    }
                    else if (idx == li.Count)
                    {
                        // expand for one
                        li.Add(value);
                    }
                    else
                    {
                        li[idx] = value;
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
                (_, error) = SetProperty(curScope, parts.Last(), value);
                if (error != null)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Return the version info of SimpleObjectMemory.
        /// </summary>
        /// <returns>A string value.</returns>
        public string Version()
        {
            return ToString();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string value.</returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(_memory, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
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
            else if (instance is JObject jobj)
            {
                jobj[property] = FunctionUtils.ConvertToJToken(value);
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
