// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

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
        public static HashSet<string> References(this Expression expression)
        {
            var walker = new ReferenceVisitor();
            expression.Accept(walker);
            walker.TerminatePath();
            return walker.References;
        }

        private static Type[] _string = new Type[] { typeof(string) };

        public static (object value, string error) AccessProperty(this object instance, string property, Expression Instance = null)
        {
            object value = null;
            string error = null;
            if (instance is IReadOnlyDictionary<string, object> dict)
            {
                if (!dict.TryGetValue(property, out value))
                {
                    error = $"{Instance} does not have {property}.";
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
                    var indexer = type.GetProperty("Item", _string);
                    if (indexer != null)
                    {
                        value = indexer.GetValue(instance, new object[] { property });
                        if (value == null)
                        {
                            error = $"{Instance} does not have {property}.";
                        }
                    }
                    else
                    {
                        error = $"{Instance} does not have {property}.";
                    }
                }
            }
            return (value, error);
        }
    }
}
