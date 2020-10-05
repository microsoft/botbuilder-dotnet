// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using AdaptiveExpressions.Converters;
using AdaptiveExpressions.Memory;
using Newtonsoft.Json;

namespace AdaptiveExpressions
{
    /// <summary>
    /// Type expected from evaluating an expression.
    /// </summary>
    [Flags]
#pragma warning disable CA1714 // Flags enums should have plural names (we can't change this without breaking binary compat)
    public enum ReturnType
#pragma warning restore CA1714 // Flags enums should have plural names
    {
        /// <summary>
        /// True or false boolean value.
        /// </summary>
        Boolean = 1,

        /// <summary>
        /// Numerical value like int, float, double, ...
        /// </summary>
        Number = 2,

        /// <summary>
        /// Any value is possible.
        /// </summary>
#pragma warning disable CA1720 // Identifier contains type name (we can't change this without breaking binary compat)
        Object = 4,
#pragma warning restore CA1720 // Identifier contains type name

        /// <summary>
        /// String value.
        /// </summary>
#pragma warning disable CA1720 // Identifier contains type name (we can't change this without breaking binary compat)
        String = 8,
#pragma warning restore CA1720 // Identifier contains type name

        /// <summary>
        /// Array value.
        /// </summary>
        Array = 16,
    }

    /// <summary>
    /// An expression which can be analyzed or evaluated to produce a value.
    /// </summary>
    /// <remarks>
    /// This provides an open-ended wrapper that supports a number of built-in functions and can also be extended at runtime.
    /// It also supports validation of the correctness of an expression and evaluation that should be exception free.
    /// </remarks>
    [JsonConverter(typeof(ExpressionConverter))]
    public class Expression
    {
        /// <summary>
        /// Dictionary of function => ExpressionEvaluator.
        /// </summary>
        /// <remarks>
        /// This is all available functions, you can add custom functions to it, but you cannot
        /// replace builtin functions.  If you clear the dictionary, it will be reset to the built in functions.
        /// </remarks>
        public static readonly FunctionTable Functions = new FunctionTable();

        /// <summary>
        /// Initializes a new instance of the <see cref="Expression"/> class.
        /// Built-in expression constructor.
        /// </summary>
        /// <param name="type">Type of built-in expression from <see cref="ExpressionType"/>.</param>
        /// <param name="children">Child expressions.</param>
        public Expression(string type, params Expression[] children)
        {
            Evaluator = Functions[type] ?? throw new SyntaxErrorException($"{type} does not have an evaluator, it's not a built-in function or a custom function.");
            Children = children;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Expression"/> class.
        /// Expression constructor.
        /// </summary>
        /// <param name="evaluator">Information about how to validate and evaluate expression.</param>
        /// <param name="children">Child expressions.</param>
        public Expression(ExpressionEvaluator evaluator, params Expression[] children)
        {
            Evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
            Children = children;
        }

        /// <summary>
        /// Gets type of expression.
        /// </summary>
        /// <value>
        /// Type of expression.
        /// </value>
        public string Type => Evaluator.Type;

        /// <summary>
        /// Gets expression evaluator.
        /// </summary>
        /// <value>
        /// expression evaluator.
        /// </value>
        public ExpressionEvaluator Evaluator { get; }

        /// <summary>
        /// Gets or sets children expressions.
        /// </summary>
        /// <value>
        /// Children expressions.
        /// </value>
#pragma warning disable CA1819 // Properties should not return arrays (we can't change this without breaking binary compat)
        public Expression[] Children { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary>
        /// Gets expected result of evaluating expression.
        /// </summary>
        /// <value>
        /// Expected result of evaluating expression.
        /// </value>
        public ReturnType ReturnType => Evaluator.ReturnType;

        /// <summary>
        /// allow a string to be implicitly assigned to an expression property.
        /// </summary>
        /// <param name="expression">string expression.</param>
#pragma warning disable CA2225 // Operator overloads have named alternates
        public static implicit operator Expression(string expression) => Expression.Parse(expression?.TrimStart('='));
#pragma warning restore CA2225 // Operator overloads have named alternates

        /// <summary>
        /// Parse an expression string into an expression object.
        /// </summary>
        /// <param name="expression">expression string.</param>
        /// <param name="lookup">Optional function lookup when parsing the expression. Default is Expression.Lookup which uses Expression.Functions table.</param>
        /// <returns>expression object.</returns>
        public static Expression Parse(string expression, EvaluatorLookup lookup = null) => new ExpressionParser(lookup ?? Expression.Lookup).Parse(expression?.TrimStart('='));

        /// <summary>
        /// Lookup a ExpressionEvaluator (function) by name.
        /// </summary>
        /// <param name="functionName">function name.</param>
        /// <returns>ExpressionEvaluator.</returns>
        public static ExpressionEvaluator Lookup(string functionName) => Functions.TryGetValue(functionName, out var function) ? function : null;

        /// <summary>
        /// Make an expression and validate it.
        /// </summary>
        /// <param name="type">Type of expression from <see cref="ExpressionType"/>.</param>
        /// <param name="children">Child expressions.</param>
        /// <returns>New expression.</returns>
        public static Expression MakeExpression(string type, params Expression[] children)
        {
            var expr = new Expression(type, children);
            expr.Validate();
            return expr;
        }

        /// <summary>
        /// Make an expression and validate it.
        /// </summary>
        /// <param name="evaluator">Information about how to validate and evaluate expression.</param>
        /// <param name="children">Child expressions.</param>
        /// <returns>New expression.</returns>
        public static Expression MakeExpression(ExpressionEvaluator evaluator, params Expression[] children)
        {
            var expr = new Expression(evaluator, children);
            expr.Validate();
            return expr;
        }

        /// <summary>
        /// Construct an expression from a <see cref="EvaluateExpressionDelegate"/>.
        /// </summary>
        /// <param name="function">Function to create an expression from.</param>
        /// <returns>New expression.</returns>
        public static Expression LambaExpression(EvaluateExpressionDelegate function)
            => new Expression(new ExpressionEvaluator(ExpressionType.Lambda, function));

        /// <summary>
        /// Construct an expression from a lambda expression over the state.
        /// </summary>
        /// <remarks>Exceptions will be caught and surfaced as an error string.</remarks>
        /// <param name="function">Lambda expression to evaluate.</param>
        /// <returns>New expression.</returns>
        public static Expression Lambda(Func<object, object> function)
            => new Expression(new ExpressionEvaluator(ExpressionType.Lambda, (expression, state, _) =>
            {
                object value = null;
                string error = null;
                try
                {
                    value = function(state);
                }
#pragma warning disable CA1031 // Do not catch general exception types (capture the exception and return in in the error)
                catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    error = e.Message;
                }

                return (value, error);
            }));

        /// <summary>
        /// Construct and validate an Set a property expression to a value expression.
        /// </summary>
        /// <param name="property">property expression.</param>
        /// <param name="value">value expression.</param>
        /// <returns>New expression.</returns>
        public static Expression SetPathToValue(Expression property, Expression value)
            => Expression.MakeExpression(ExpressionType.SetPathToValue, property, value);

        /// <summary>
        /// Construct and validate an Set a property expression to a value expression.
        /// </summary>
        /// <param name="property">property expression.</param>
        /// <param name="value">value object.</param>
        /// <returns>New expression.</returns>
        public static Expression SetPathToValue(Expression property, object value)
        {
            if (value is Expression)
            {
                return Expression.MakeExpression(ExpressionType.SetPathToValue, property, (Expression)value);
            }
            else
            {
                return Expression.MakeExpression(ExpressionType.SetPathToValue, property, ConstantExpression(value));
            }
        }

        /// <summary>
        /// Construct and validate an Equals expression.
        /// </summary>
        /// <param name="children">Child clauses.</param>
        /// <returns>New expression.</returns>
        public static Expression EqualsExpression(params Expression[] children)
        => Expression.MakeExpression(ExpressionType.Equal, children);

        /// <summary>
        /// Construct and validate an And expression.
        /// </summary>
        /// <param name="children">Child clauses.</param>
        /// <returns>New expression.</returns>
        public static Expression AndExpression(params Expression[] children)
        {
            if (children.Length > 1)
            {
                return Expression.MakeExpression(ExpressionType.And, children);
            }

            return children.Single();
        }

        /// <summary>
        /// Construct and validate an Or expression.
        /// </summary>
        /// <param name="children">Child clauses.</param>
        /// <returns>New expression.</returns>
        public static Expression OrExpression(params Expression[] children)
        {
            if (children.Length > 1)
            {
                return Expression.MakeExpression(ExpressionType.Or, children);
            }

            return children.Single();
        }

        /// <summary>
        /// Construct and validate a Not expression.
        /// </summary>
        /// <param name="child">Child clauses.</param>
        /// <returns>New expression.</returns>
        public static Expression NotExpression(Expression child)
            => Expression.MakeExpression(ExpressionType.Not, child);

        /// <summary>
        /// Construct a constant expression.
        /// </summary>
        /// <param name="value">Constant value.</param>
        /// <returns>New expression.</returns>
        public static Expression ConstantExpression(object value)
            => new Constant(value);

        /// <summary>
        /// Construct and validate a property accessor.
        /// </summary>
        /// <param name="property">Property to lookup.</param>
        /// <param name="instance">Expression to get instance that contains property or null for global state.</param>
        /// <returns>New expression.</returns>
        public static Expression Accessor(string property, Expression instance = null)
            => instance == null
            ? MakeExpression(ExpressionType.Accessor, ConstantExpression(property))
            : MakeExpression(ExpressionType.Accessor, ConstantExpression(property), instance);

        /// <summary>
        /// Do a deep equality between expressions.
        /// </summary>
        /// <param name="other">Other expression.</param>
        /// <returns>True if expressions are the same.</returns>
        public virtual bool DeepEquals(Expression other)
        {
            var eq = false;
            if (other != null)
            {
                eq = this.Type == other.Type;
                if (eq)
                {
                    eq = this.Children.Length == other.Children.Length;
                    if (this.Type == ExpressionType.And || this.Type == ExpressionType.Or)
                    {
                        // And/Or do not depend on order
                        for (var i = 0; eq && i < this.Children.Length; ++i)
                        {
                            var primary = this.Children[i];
                            var found = false;
                            for (var j = 0; j < this.Children.Length; ++j)
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
                        for (var i = 0; eq && i < this.Children.Length; ++i)
                        {
                            eq = this.Children[i].DeepEquals(other.Children[i]);
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
        /// <returns>List of the static reference paths.</returns>
        public IReadOnlyList<string> References()
        {
            var (path, refs) = ReferenceWalk(this);
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
        /// <returns>Accessor path of expression which is a potential partial path and the full path found so far.</returns>
        public (string path, HashSet<string> references) ReferenceWalk(Expression expression, Func<Expression, bool> extension = null)
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
                    var nonLocalRefs2 = refs2.Where(x => !(x.Equals(iteratorName, StringComparison.Ordinal) || x.StartsWith(iteratorName + '.', StringComparison.Ordinal) || x.StartsWith(iteratorName + '[', StringComparison.Ordinal)))
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

        /// <summary>
        /// Validate immediate expression.
        /// </summary>
        public void Validate() => Evaluator.ValidateExpression(this);

        /// <summary>
        /// Recursively validate the expression tree.
        /// </summary>
        public void ValidateTree()
        {
            Validate();
            foreach (var child in Children)
            {
                child.ValidateTree();
            }
        }

        /// <summary>
        /// Evaluate the expression.
        /// </summary>
        /// <param name="state">
        /// Global state to evaluate accessor expressions against.  Can be <see cref="System.Collections.Generic.IDictionary{String, Object}"/>,
        /// <see cref="System.Collections.IDictionary"/> otherwise reflection is used to access property and then indexer.
        /// </param>
        /// <param name="options">Options used in the evaluation. </param>
        /// <returns>Computed value and an error string.  If the string is non-null, then there was an evaluation error.</returns>
        public (object value, string error) TryEvaluate(object state, Options options = null)
            => this.TryEvaluate<object>(MemoryFactory.Create(state), options);

        /// <summary>
        /// Evaluate the expression.
        /// </summary>
        /// <param name="state">
        /// Global state to evaluate accessor expressions against.  Can be <see cref="System.Collections.Generic.IDictionary{String, Object}"/>,
        /// <see cref="System.Collections.IDictionary"/> otherwise reflection is used to access property and then indexer.
        /// </param>
        /// <param name="options">Options used in the evaluation. </param>
        /// <returns>Computed value and an error string.  If the string is non-null, then there was an evaluation error.</returns>
        public (object value, string error) TryEvaluate(IMemory state, Options options = null)
            => this.TryEvaluate<object>(state, options);

        /// <summary>
        /// Evaluate the expression.
        /// </summary>
        /// <typeparam name="T">type of result of the expression.</typeparam>
        /// <param name="state">
        /// Global state to evaluate accessor expressions against.  Can be <see cref="System.Collections.Generic.IDictionary{String, Object}"/>,
        /// <see cref="System.Collections.IDictionary"/> otherwise reflection is used to access property and then indexer.
        /// </param>
        /// <param name="options">Options used in the evaluation. </param>
        /// <returns>Computed value and an error string.  If the string is non-null, then there was an evaluation error.</returns>
        public (T value, string error) TryEvaluate<T>(object state, Options options = null)
        => this.TryEvaluate<T>(MemoryFactory.Create(state), options);

        /// <summary>
        /// Evaluate the expression.
        /// </summary>
        /// <typeparam name="T">type of result of the expression.</typeparam>
        /// <param name="state">
        /// Global state to evaluate accessor expressions against.  Can be <see cref="System.Collections.Generic.IDictionary{String, Object}"/>,
        /// <see cref="System.Collections.IDictionary"/> otherwise reflection is used to access property and then indexer.
        /// </param>
        /// <param name="options">Options used in the evaluation. </param>
        /// <returns>Computed value and an error string.  If the string is non-null, then there was an evaluation error.</returns>
        public (T value, string error) TryEvaluate<T>(IMemory state, Options options = null)
        {
            var opts = options ?? new Options();
            var (result, error) = Evaluator.TryEvaluate(this, state, opts);
            if (error != null)
            {
                return (default(T), error);
            }

            if (result is T)
            {
                return ((T)result, error);
            }

            try
            {
                if (typeof(T) == typeof(object))
                {
                    if (result == null)
                    {
                        return (default(T), error);
                    }

                    return ((T)result, error);
                }

                if (typeof(T) == typeof(string))
                {
                    if (result == null)
                    {
                        return (default(T), null);
                    }

                    return ((T)(object)result.ToString(), error);
                }

                if (typeof(T) == typeof(bool))
                {
                    return ((T)(object)Convert.ToBoolean(result, CultureInfo.InvariantCulture), error);
                }

                if (typeof(T) == typeof(byte))
                {
                    return ((T)(object)Convert.ToByte(result, CultureInfo.InvariantCulture), (Convert.ToByte(result, CultureInfo.InvariantCulture) == Convert.ToDouble(result, CultureInfo.InvariantCulture)) ? null : Error<T>(result));
                }

                if (typeof(T) == typeof(short))
                {
                    return ((T)(object)Convert.ToInt16(result, CultureInfo.InvariantCulture), (Convert.ToInt16(result, CultureInfo.InvariantCulture) == Convert.ToDouble(result, CultureInfo.InvariantCulture)) ? null : Error<T>(result));
                }

                if (typeof(T) == typeof(int))
                {
                    return ((T)(object)Convert.ToInt32(result, CultureInfo.InvariantCulture), (Convert.ToInt32(result, CultureInfo.InvariantCulture) == Convert.ToDouble(result, CultureInfo.InvariantCulture)) ? null : Error<T>(result));
                }

                if (typeof(T) == typeof(long))
                {
                    return ((T)(object)Convert.ToInt64(result, CultureInfo.InvariantCulture), (Convert.ToInt64(result, CultureInfo.InvariantCulture) == Convert.ToDouble(result, CultureInfo.InvariantCulture)) ? null : Error<T>(result));
                }

                if (typeof(T) == typeof(ushort))
                {
                    return ((T)(object)Convert.ToUInt16(result, CultureInfo.InvariantCulture), (Convert.ToUInt16(result, CultureInfo.InvariantCulture) == Convert.ToDouble(result, CultureInfo.InvariantCulture)) ? null : Error<T>(result));
                }

                if (typeof(T) == typeof(uint))
                {
                    return ((T)(object)Convert.ToUInt32(result, CultureInfo.InvariantCulture), (Convert.ToUInt32(result, CultureInfo.InvariantCulture) == Convert.ToDouble(result, CultureInfo.InvariantCulture)) ? null : Error<T>(result));
                }

                if (typeof(T) == typeof(ulong))
                {
                    return ((T)(object)Convert.ToUInt64(result, CultureInfo.InvariantCulture), (Convert.ToUInt64(result, CultureInfo.InvariantCulture) == Convert.ToDouble(result, CultureInfo.InvariantCulture)) ? null : Error<T>(result));
                }

                if (typeof(T) == typeof(float))
                {
                    return ((T)(object)Convert.ToSingle(Convert.ToDecimal(result, CultureInfo.InvariantCulture)), null);
                }

                if (typeof(T) == typeof(double))
                {
                    return ((T)(object)Convert.ToDouble(Convert.ToDecimal(result, CultureInfo.InvariantCulture)), null);
                }

                if (result == null)
                {
                    return (default(T), error);
                }

                return (JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(result)), null);
            }
#pragma warning disable CA1031 // Do not catch general exception types (just return an error)
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                return (default(T), Error<T>(result));
            }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string value of this Expression.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            var valid = false;

            // Special support for memory paths
            if (Type == ExpressionType.Accessor && Children.Length >= 1)
            {
                if (Children[0] is Constant cnst
                    && cnst.Value is string prop)
                {
                    if (Children.Length == 1)
                    {
                        valid = true;
                        builder.Append(prop);
                    }
                    else if (Children.Length == 2)
                    {
                        valid = true;
                        builder.Append(Children[1].ToString());
                        builder.Append('.');
                        builder.Append(prop);
                    }
                }
            }

            // Element support
            else if (Type == ExpressionType.Element && Children.Length == 2)
            {
                valid = true;
                builder.Append(Children[0].ToString());
                builder.Append('[');
                builder.Append(Children[1].ToString());
                builder.Append(']');
            }

            // Generic version
            if (!valid)
            {
                var infix = Type.Length > 0 && !char.IsLetter(Type[0]) && Children.Length >= 2;
                if (!infix)
                {
                    builder.Append(Type);
                }

                builder.Append('(');
                var first = true;
                foreach (var child in Children)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        if (infix)
                        {
                            builder.Append(' ');
                            builder.Append(Type);
                            builder.Append(' ');
                        }
                        else
                        {
                            builder.Append(", ");
                        }
                    }

                    builder.Append(child.ToString());
                }

                builder.Append(')');
            }

            return builder.ToString();
        }

        private string Error<T>(object result)
        {
            return $"'{result}' is not of type {typeof(T).Name}";
        }

        /// <summary>
        /// FunctionTable is a dictionary which merges BuiltinFunctions.Functions with a CustomDictionary.
        /// </summary>
#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat)
#pragma warning disable CA1710 // Identifiers should have correct suffix (we can't change this without breaking binary compat)
        public class FunctionTable : IDictionary<string, ExpressionEvaluator>
#pragma warning restore CA1710 // Identifiers should have correct suffix
#pragma warning restore CA1034 // Nested types should not be visible
        {
            private readonly ConcurrentDictionary<string, ExpressionEvaluator> _customFunctions = new ConcurrentDictionary<string, ExpressionEvaluator>();

            /// <summary>
            /// Gets a collection of string values that represent the keys of the StandardFunctions. 
            /// </summary>
            /// <value> A list of string values.</value>
            public ICollection<string> Keys => ExpressionFunctions.StandardFunctions.Keys.Concat(_customFunctions.Keys).ToList();

            /// <summary>
            /// Gets a collection of ExpressionEvaluator which is the value of the StandardFunctions.
            /// </summary>
            /// <value>A list of ExpressionEvaluator.</value>
            public ICollection<ExpressionEvaluator> Values => ExpressionFunctions.StandardFunctions.Values.Concat(_customFunctions.Values).ToList();

            /// <summary>
            /// Gets a value of the total number of StandardFunctions and user custom functions.
            /// </summary>
            /// <value>An integer value.</value>
            public int Count => ExpressionFunctions.StandardFunctions.Count + _customFunctions.Count;

            /// <summary>
            /// Gets a value indicating whether the FunctionTable is readonly.
            /// </summary>
            /// <value>A boolean value indicating whether the FunctionTable is readonly.</value>
            public bool IsReadOnly => false;

            /// <summary>
            /// Gets a value of ExpressionEvaluator corresponding to the given key.
            /// </summary>
            /// <param name="key">A string value of function name.</param>
            /// <returns>An ExpressionEvaluator.</returns>
            public ExpressionEvaluator this[string key]
            {
                get
                {
                    if (ExpressionFunctions.StandardFunctions.TryGetValue(key, out var function))
                    {
                        return function;
                    }

                    if (_customFunctions.TryGetValue(key, out function))
                    {
                        return function;
                    }

                    return null;
                }

                set
                {
                    if (ExpressionFunctions.StandardFunctions.ContainsKey(key))
                    {
                        throw new NotSupportedException("You can't overwrite a built in function.");
                    }

                    _customFunctions[key] = value;
                }
            }

            /// <summary>
            /// Inserts a mapping of a string key to ExpressionEvaluator into FunctionTable.
            /// </summary>
            /// <param name="key">The function name to be added.</param>
            /// <param name="value">The value of the ExpressionEvaluator to be added.</param>
            public void Add(string key, ExpressionEvaluator value) => this[key] = value;

            /// <summary>
            /// Inserts a mapping of a string key to user customized function into FunctionTable.
            /// </summary>
            /// <param name="key">The key of function name to be added.</param>
            /// <param name="func">The value of the user customized function to be added.</param>
            public void Add(string key, Func<IReadOnlyList<dynamic>, object> func)
            {
                Add(key, new ExpressionEvaluator(key, FunctionUtils.Apply(func)));
            }

            /// <summary>
            /// Inserts a mapping of a string key to ExpressionEvaluator into FunctionTable from a key value pair.
            /// </summary>
            /// <param name="item">A key value pair of string to ExpressionEvaluator.</param>
            public void Add(KeyValuePair<string, ExpressionEvaluator> item) => this[item.Key] = item.Value;

            /// <summary>
            /// Clears the user customized functions.
            /// </summary>
            public void Clear() => _customFunctions.Clear();

            /// <summary>
            /// Determines whether FunctionTable contains a given key value pair of string to ExpressionEvaluator.
            /// </summary>
            /// <param name="item">A key value pair of string to ExpressionEvaluator.</param>
            /// <returns>
            /// A boolean value indicating  whether the key-value pair is in the FunctionTable.
            /// Retuens True if the key-value pair is contained, otherwise returns False.
            /// </returns>
            public bool Contains(KeyValuePair<string, ExpressionEvaluator> item) => ExpressionFunctions.StandardFunctions.Contains(item) || _customFunctions.Contains(item);

            /// <summary>
            /// Determines if the FunctionTable contains a given string key.
            /// </summary>
            /// <param name="key">A string key.</param>
            /// <returns>
            /// A boolean value indicating  whether the key is in the FunctionTable.
            /// Retuens True if the key is contained, otherwise returns False.
            /// </returns>
            public bool ContainsKey(string key) => ExpressionFunctions.StandardFunctions.ContainsKey(key) || _customFunctions.ContainsKey(key);

            /// <summary>
            /// Not implemented.
            /// </summary>
            /// <param name="array">An array of string values.</param>
            /// <param name="arrayIndex">An integer of index.</param>
            public void CopyTo(KeyValuePair<string, ExpressionEvaluator>[] array, int arrayIndex) => throw new NotImplementedException();

            /// <summary>
            /// Generates an enumerator through all standard functions.
            /// </summary>
            /// <returns>An enumerator of standard functions.</returns>
            public IEnumerator<KeyValuePair<string, ExpressionEvaluator>> GetEnumerator() => ExpressionFunctions.StandardFunctions.Concat(_customFunctions).GetEnumerator();

            /// <summary>
            /// Removes a specified key from user customized functions.
            /// </summary>
            /// <param name="key">A string key of function name.</param>
            /// <returns>A boolean value indicating  whether the key is successfully removed.</returns>
            public bool Remove(string key) => _customFunctions.TryRemove(key, out var oldVal);

            /// <summary>
            /// Removes a specified key value pair from user customized functions.
            /// </summary>
            /// <param name="item">A key value pair of string to ExpressionEvaluator.</param>
            /// <returns>A boolean value indicating  whether the key is successfully removed.</returns>
            public bool Remove(KeyValuePair<string, ExpressionEvaluator> item) => Remove(item.Key);

            /// <summary>
            /// Attempts to get the value associated with the specified key from the FunctionTable.
            /// </summary>
            /// <param name="key">The key of the value to get.</param>
            /// <param name="value">When this method returns, contains the object from the FunctionTable
            /// that has the specified key, or the default value of the type if the operation failed.</param>
            /// <returns>A boolean value indicating  whether the value is successfully obtained.</returns>
            public bool TryGetValue(string key, out ExpressionEvaluator value)
            {
                if (ExpressionFunctions.StandardFunctions.TryGetValue(key, out value))
                {
                    return true;
                }

                if (_customFunctions.TryGetValue(key, out value))
                {
                    return true;
                }

                return false;
            }

            IEnumerator IEnumerable.GetEnumerator() => ExpressionFunctions.StandardFunctions.Concat(_customFunctions).GetEnumerator();
        }
    }
}
