// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Expressions
{
    /// <summary>
    /// Extension methods for detecting or value testing of various types.
    /// </summary>
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
                        if (expr.Type == ExpressionType.And || expr.Type == ExpressionType.Or)
                        {
                            // And/Or do not depend on order
                            for (var i = 0; eq && i < expr.Children.Count(); ++i)
                            {
                                var primary = expr.Children[i];
                                var found = false;
                                for (var j = 0; j < expr.Children.Count(); ++j)
                                {
                                    if (primary.DeepEquals(other.Children[j]))
                                    {
                                        found = true;
                                        break;
                                    }
                                }

                                eq = found;
                            }
                        }
                        else
                        {
                            for (var i = 0; eq && i < expr.Children.Count(); ++i)
                            {
                                eq = expr.Children[i].DeepEquals(other.Children[i]);
                            }
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
        /// <returns>List of the static reference paths.</returns>
        public static IReadOnlyList<string> References(this Expression expression)
        {
            var (path, refs) = ReferenceWalk(expression);
            if (path != null)
            {
                refs.Add(path);
            }

            return refs.ToList();
        }

        /// <summary>
        /// Walking function for identifying static memory references in an expression.
        /// </summary>
        /// <param name="expression">Expression to analyze.</param>
        /// <param name="extension">If present, called to override lookup for things like template expansion.</param>
        /// <returns>Accessor path of expression which is a potential partial path and the fullpath found so far.</returns>
        public static (string path, HashSet<string> references) ReferenceWalk(Expression expression, Func<Expression, bool> extension = null)
        {
            string path = null;
            var refs = new HashSet<string>();
            if (extension == null || !extension(expression))
            {
                var children = expression.Children;
                if (expression.Type == ExpressionType.Accessor)
                {
                    var prop = (string)((Constant)children[0]).Value;

                    if (children.Length == 1)
                    {
                        path = prop;
                    }

                    if (children.Length == 2)
                    {
                        (path, refs) = ReferenceWalk(children[1], extension);
                        if (path != null)
                        {
                            path = path + "." + prop;
                        }

                        // if path is null we still keep it null, won't append prop
                        // because for example, first(items).x should not return x as refs
                    }
                }
                else if (expression.Type == ExpressionType.Element)
                {
                    (path, refs) = ReferenceWalk(children[0], extension);
                    if (path != null)
                    {
                        if (children[1] is Constant cnst)
                        {
                            if (cnst.ReturnType == ReturnType.String)
                            {
                                path += $".{cnst.Value}";
                            }
                            else
                            {
                                path += $"[{cnst.Value}]";
                            }
                        }
                        else
                        {
                            refs.Add(path);
                        }
                    }

                    var (idxPath, refs1) = ReferenceWalk(children[1], extension);
                    refs.UnionWith(refs1);

                    if (idxPath != null)
                    {
                        refs.Add(idxPath);
                    }
                }
                else if (expression.Type == ExpressionType.Foreach ||
                         expression.Type == ExpressionType.Where ||
                         expression.Type == ExpressionType.Select)
                {
                    var (child0Path, refs0) = ReferenceWalk(children[0], extension);
                    if (child0Path != null)
                    {
                        refs0.Add(child0Path);
                    }

                    var (child2Path, refs2) = ReferenceWalk(children[2], extension);
                    if (child2Path != null)
                    {
                        refs2.Add(child2Path);
                    }

                    var iteratorName = (string)(children[1].Children[0] as Constant).Value;

                    // filter references found in children 2 with iterator name
                    var nonLocalRefs2 = refs2.Where(x => !(x.Equals(iteratorName) || x.StartsWith(iteratorName + '.') || x.StartsWith(iteratorName + '[')))
                                             .ToList();

                    refs.UnionWith(refs0);
                    refs.UnionWith(nonLocalRefs2);
                }
                else
                {
                    foreach (var child in expression.Children)
                    {
                        var (childPath, refs0) = ReferenceWalk(child, extension);
                        refs.UnionWith(refs0);
                        if (childPath != null)
                        {
                            refs.Add(childPath);
                        }
                    }
                }
            }

            return (path, refs);
        }
    }
}
