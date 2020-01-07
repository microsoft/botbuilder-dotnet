// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Expressions.Memory
{
    public class SimpleObjectMemory : IMemory
    {
        private object memory = null;
        private int version = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleObjectMemory"/> class.
        /// This wraps a simple object as IMemory.
        /// </summary>
        /// <param name="memory">the object to wrap.</param>
        public SimpleObjectMemory(object memory)
        {
            this.memory = memory;
        }

        public static IMemory Wrap(object obj)
        {
            if (obj is IMemory)
            {
                return (IMemory)obj;
            }

            return new SimpleObjectMemory(obj);
        }

        public bool TryGetValue(string path, out object value)
        {
            value = null;
            if (memory == null || path.Length == 0 || (path[0] != '[' && !char.IsLetter(path[0])))
            {
                return false;
            }

            var parts = path.Split(".[]".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Trim('\'', '"'))
                            .ToArray(); 

            var curScope = memory;

            foreach (var part in parts)
            {
                string error = null;
                if (int.TryParse(part, out var idx) && BuiltInFunctions.TryParseList(curScope, out var li))
                {
                    (value, error) = BuiltInFunctions.AccessIndex(li, idx);
                    if (error != null)
                    {
                        return false;
                    }
                }
                else
                {
                    if (!BuiltInFunctions.TryAccessProperty(curScope, part, out value))
                    {
                        return false;
                    }
                }

                curScope = value;
            }

            return true;
        }

        // In this simple object scope, we don't allow you to set a path in which some parts in middle don't exist
        // for example
        // if you set dialog.a.b = x, but dialog.a don't exist, this will result in an error
        // because we can't and shouldn't smart create structure in the middle
        // you can implement a customzied Scope that support such behavior
        public void SetValue(string path, object value)
        {
            if (memory == null)
            {
                return;
            }

            var parts = path.Split(".[]".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Trim('\'', '"'))
                            .ToArray();

            var curScope = memory;
            var curPath = string.Empty; // valid path so far
            string error = null;

            // find the 2nd last value, ie, the container
            for (var i = 0; i < parts.Length - 1; i++)
            {
                if (int.TryParse(parts[i], out var index) && BuiltInFunctions.TryParseList(curScope, out var li))
                {
                    curPath += $"[{parts[i]}]";
                    (curScope, error) = BuiltInFunctions.AccessIndex(li, index);
                }
                else
                {
                    curPath += $".{parts[i]}";
                    if (BuiltInFunctions.TryAccessProperty(curScope, parts[i], out var newScope))
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
                    return;
                }
            }
            else
            {
                (_, error) = BuiltInFunctions.SetProperty(curScope, parts.Last(), value);
                if (error != null)
                {
                    return;
                }
            }

            // Update the version once memory has been updated
            version++;
        }

        public string Version()
        {
            return version.ToString();
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(memory, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }
    }
}
