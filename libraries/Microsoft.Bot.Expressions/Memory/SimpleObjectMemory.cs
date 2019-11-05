using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Expressions.Memory
{
    internal class SimpleObjectMemory : IMemory
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
            string error = null;

            // find the 2nd last value, ie, the container
            for (var i = 0; i < parts.Length - 1; i++)
            {
                if (int.TryParse(parts[i], out var index))
                {
                    (curScope, error) = BuiltInFunctions.AccessIndex(curScope, index);
                }
                else
                {
                    (curScope, error) = BuiltInFunctions.AccessProperty(curScope, parts[i]);
                }

                if (error != null)
                {
                    return (null, error);
                }
            }

            if (curScope == null)
            {
                return (null, $"Some parts in the middle of path doesn't exist: {path}");
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
                BuiltInFunctions.SetProperty(curScope, parts.Last(), value);
            }

            return (BuiltInFunctions.ResolveValue(value), null);
        }
    }
}
