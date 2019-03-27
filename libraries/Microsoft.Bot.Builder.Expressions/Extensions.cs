// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Builder.Expressions
{
    public static partial class Extensions
    {
        public static bool IsNumber(this object value)
        {
            return value is sbyte
                    || value is byte
                    || value is short
                    || value is ushort
                    || value is int
                    || value is uint
                    || value is long
                    || value is ulong
                    || value is float
                    || value is double
                    || value is decimal;
        }

        /// <summary>
        /// Return the static reference paths to memory.
        /// </summary>
        /// <remarks>
        /// Return all static paths to memory.  If there is a computed element index, then the path is terminated there, 
        /// but you might get paths from the computed part as well.
        /// </remarks>
        /// <param name="expression">Expresion to get references from.</param>
        /// <returns>Hash set of the static reference paths.</returns>
        public static IReadOnlyList<string> References(this Expression expression)
        {
            var references = new HashSet<string>();
            var path = ReferenceWalk(expression, references);
            if (path != null)
            {
                references.Add(path);
            }
            return references.ToList();
        }

        public static string ReferenceWalk(Expression expression, HashSet<string> references, Func<Expression, bool> extension = null)
        {
            string path = null;
            if (extension == null || !extension(expression))
            {
                var children = expression.Children;
                if (expression.Type == ExpressionType.Accessor)
                {
                    if (children.Length == 2)
                    {
                        path = ReferenceWalk(children[1], references, extension);
                    }
                    var prop = (string)((Constant)children[0]).Value;
                    path = (path == null ? prop : path + "." + prop);
                }
                else if (expression.Type == ExpressionType.Element)
                {
                    path = ReferenceWalk(children[0], references, extension);
                    if (path != null)
                    {
                        if (children[1] is Constant cnst)
                        {
                            path += $"[{cnst.Value}]";
                        }
                        else
                        {
                            references.Add(path);
                        }
                    }
                    var idxPath = ReferenceWalk(children[1], references, extension);
                    if (idxPath != null)
                    {
                        references.Add(idxPath);
                    }
                }
                else
                {
                    foreach (var child in expression.Children)
                    {
                        var childPath = ReferenceWalk(child, references, extension);
                        if (childPath != null)
                        {
                            references.Add(childPath);
                        }
                    }
                }
            }
            return path;
        }

        private static Type[] _string = new Type[] { typeof(string) };

        public static (object value, string error) AccessProperty(this object instance, string property, Expression expression = null)
        {
            // NOTE: This returns null rather than an error if property is not present
            object value = null;
            string error = null;
            if (instance != null)
            {
                if (instance is IDictionary<string, object> idict)
                {
                    idict.TryGetValue(property, out value);
                }
                else if (instance is System.Collections.IDictionary dict)
                {
                    if (dict.Contains(property))
                    {
                        value = dict[property];
                    }
                }
                else
                {
                    // Use reflection
                    var type = instance.GetType();
                    var prop = type.GetProperty(property);
                    if (prop != null)
                    {
                        value = prop.GetValue(instance);
                    }
                    else
                    {
                        // This will work on JSON objects
                        var indexer = type.GetProperty("Item", _string);
                        if (indexer != null)
                        {
                            value = indexer.GetValue(instance, new object[] { property });
                        }
                    }
                }
            }
            return (value, error);
        }
    }
}
