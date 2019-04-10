// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Expressions
{
    public static partial class Extensions
    {
        /// <summary>
        /// Test an object to see if it is a numeric type.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <returns>True if numeric type.</returns>
        public static bool IsNumber(this object value)
            => value is sbyte
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

        /// <summary>
        /// Test an object to see if it is an integer type.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <returns>True if numeric type.</returns>
        public static bool IsInteger(this object value)
            => value is sbyte
            || value is byte
            || value is short
            || value is ushort
            || value is int
            || value is uint
            || value is long
            || value is ulong;

        /// <summary>
        /// Do a deep equality between expressions.
        /// </summary>
        /// <param name="expr">Base expression.</param>
        /// <param name="other">Other expression.</param>
        /// <returns>True if expressions are the same.</returns>
        public static bool DeepEquals(this Expression expr, Expression other)
        {
            var eq = true;
            if (expr != null && other != null)
            {
                eq = expr.Type == other.Type;
                if (eq)
                {
                    if (expr.Type == ExpressionType.Constant)
                    {
                        var val = ((Constant)expr).Value;
                        var otherVal = ((Constant)other).Value;
                        eq = val == otherVal || (val != null && val.Equals(otherVal));
                    }
                    else
                    {
                        eq = expr.Children.Count() == other.Children.Count();
                        for (var i = 0; eq && i < expr.Children.Count(); ++i)
                        {
                            eq = expr.Children[i].DeepEquals(other.Children[i]);
                        }
                    }
                }
            }
            return eq;
        }

        /// <summary>
        /// Return the static reference paths to memory.
        /// </summary>
        /// <remarks>
        /// Return all static paths to memory.  If there is a computed element index, then the path is terminated there, 
        /// but you might get other paths from the computed part as well.
        /// </remarks>
        /// <param name="expression">Expression to get references from.</param>
        /// <returns>Hash set of the static reference paths.</returns>
        public static IReadOnlyList<string> References(this Expression expression)
        {
            var references = new HashSet<string>();
            var path = ReferenceWalk(expression, references);
            if (path != null)
            {
                references.Add(path);
            }

            var filteredReferences = new HashSet<string>();

            references.Where(x => !x.StartsWith("$local.")).ToList().ForEach(x =>
            {
                if (x.StartsWith("$global."))
                {
                    filteredReferences.Add(x.Substring(8));
                }
                else
                {
                    filteredReferences.Add(x);
                }
            });

            return filteredReferences.ToList();
        }

        /// <summary>
        /// Walking function for identifying static memory references in an expression.
        /// </summary>
        /// <param name="expression">Expression to analyze.</param>
        /// <param name="references">Tracking for references found.</param>
        /// <param name="extension">If present, called to override lookup for things like template expansion.</param>
        /// <returns>Accessor path of expression.</returns>
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

        /// <summary>
        /// Lookup a property in IDictionary, JObject or through reflection.
        /// </summary>
        /// <param name="instance">Instance with property.</param>
        /// <param name="property">Property to lookup.</param>
        /// <param name="expression">Expression that generated instance.</param>
        /// <returns>Value and error information if any.</returns>
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
                else if (instance is JObject jobj)
                {
                    if (jobj.TryGetValue(property, out var jtoken))
                    {
                        if (jtoken is JArray jarray)
                        {
                            value = jarray.ToArray<object>();
                        }
                        else if (jtoken is JValue jvalue)
                        {
                            value = jvalue.Value;
                        }
                        else value = jtoken;
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
                }
            }
            return (value, error);
        }
    }
}
