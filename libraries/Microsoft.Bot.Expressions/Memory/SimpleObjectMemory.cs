// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Expressions.Memory
{
    public class SimpleObjectMemory : IMemory
    {
        private object memory = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleObjectMemory"/> class.
        /// This wraps a simple object as IMemory.
        /// </summary>
        /// <param name="memory">the object to wrap.</param>
        public SimpleObjectMemory(object memory)
        {
            this.memory = memory;
        }

        public (object value, string error) GetValue(string path)
        {
            var parts = path.Split(".[]".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            object value = null;
            var curScope = memory;

            foreach (string part in parts)
            {
                string error = null;
                if (int.TryParse(part, out var idx))
                {
                    (value, error) = BuiltInFunctions.AccessIndex(curScope, idx);
                }
                else
                {
                    (value, error) = BuiltInFunctions.AccessProperty(curScope, part);
                }

                if (error != null)
                {
                    return (null, error);
                }

                curScope = value;
            }

            return (value, null);
        }

        // In this simple object scope, we don't allow you to set a path in which some parts in middle don't exist
        // for example
        // if you set dialog.a.b = x, but dialog.a don't exist, this will result in an error
        // because we can't and shouldn't smart create structure in the middle
        // you can implement a customzied Scope that support such behavior
        public (object value, string error) SetValue(string path, object value)
        {
            var parts = path.Split(".[]".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            var curScope = memory;
            var curPath = string.Empty; // valid path so far
            string error = null;

            // find the 2nd last value, ie, the container
            for (var i = 0; i < parts.Length - 1; i++)
            {
                if (int.TryParse(parts[i], out var index))
                {
                    curPath += $"{parts[i]}";
                    (curScope, error) = BuiltInFunctions.AccessIndex(curScope, index);
                }
                else
                {
                    curPath += $".{parts[i]}";
                    (curScope, error) = BuiltInFunctions.AccessProperty(curScope, parts[i]);
                }

                if (error != null)
                {
                    return (null, error);
                }

                if (curScope == null)
                {
                    curPath = curPath.TrimStart('.');
                    return (null, $"Can set value to path: '{path}', reason: '{curPath}' is null");
                }
            }

            // set the last value
            if (int.TryParse(parts.Last(), out var idx))
            {
                if (BuiltInFunctions.TryParseList(curScope, out var li))
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
                    return (null, error);
                }
            }
            else
            {
                (_, error) = BuiltInFunctions.SetProperty(curScope, parts.Last(), value);
                if (error != null)
                {
                    return (null, $"Can set value to path: '{path}', reason: {error}");
                }
            }

            return (BuiltInFunctions.ResolveValue(value), null);
        }

        public override string ToString()
        {
            return memory?.ToString();
        }
    }
}
