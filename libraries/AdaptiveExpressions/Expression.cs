// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
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
    public enum ReturnType
    {
        /// <summary>
        /// True or false boolean value.
        /// </summary>
        Boolean,

        /// <summary>
        /// Numerical value like int, float, double, ...
        /// </summary>
        Number,

        /// <summary>
        /// Any value is possible.
        /// </summary>
        Object,

        /// <summary>
        /// String value.
        /// </summary>
        String,
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
        public static readonly IDictionary<string, ExpressionEvaluator> Functions = new FunctionTable();

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
        public Expression[] Children { get; set; }

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
        public static implicit operator Expression(string expression) => Expression.Parse(expression);

        /// <summary>
        /// Parse an expression string into an expression object.
        /// </summary>
        /// <param name="expression">expression string.</param>
        /// <param name="lookup">Optional function lookup when parsing the expression. Default is Expression.Lookup which uses Expression.Functions table.</param>
        /// <returns>expression object.</returns>
        public static Expression Parse(string expression, EvaluatorLookup lookup = null) => new ExpressionParser(lookup ?? Expression.Lookup).Parse(expression);

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
            => new Expression(new ExpressionEvaluator(ExpressionType.Lambda, (expression, state) =>
            {
                object value = null;
                string error = null;
                try
                {
                    value = function(state);
                }
                catch (Exception e)
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
            if (children.Count() > 1)
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
            if (children.Count() > 1)
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
        /// <returns>Computed value and an error string.  If the string is non-null, then there was an evaluation error.</returns>
        public (object value, string error) TryEvaluate(object state)
            => this.TryEvaluate<object>(MemoryFactory.Create(state));

        /// <summary>
        /// Evaluate the expression.
        /// </summary>
        /// <param name="state">
        /// Global state to evaluate accessor expressions against.  Can be <see cref="System.Collections.Generic.IDictionary{String, Object}"/>,
        /// <see cref="System.Collections.IDictionary"/> otherwise reflection is used to access property and then indexer.
        /// </param>
        /// <returns>Computed value and an error string.  If the string is non-null, then there was an evaluation error.</returns>
        public (object value, string error) TryEvaluate(IMemory state)
            => this.TryEvaluate<object>(state);

        /// <summary>
        /// Evaluate the expression.
        /// </summary>
        /// <typeparam name="T">type of result of the expression.</typeparam>
        /// <param name="state">
        /// Global state to evaluate accessor expressions against.  Can be <see cref="System.Collections.Generic.IDictionary{String, Object}"/>,
        /// <see cref="System.Collections.IDictionary"/> otherwise reflection is used to access property and then indexer.
        /// </param>
        /// <returns>Computed value and an error string.  If the string is non-null, then there was an evaluation error.</returns>
        public (T value, string error) TryEvaluate<T>(object state)
        => this.TryEvaluate<T>(MemoryFactory.Create(state));

        /// <summary>
        /// Evaluate the expression.
        /// </summary>
        /// <typeparam name="T">type of result of the expression.</typeparam>
        /// <param name="state">
        /// Global state to evaluate accessor expressions against.  Can be <see cref="System.Collections.Generic.IDictionary{String, Object}"/>,
        /// <see cref="System.Collections.IDictionary"/> otherwise reflection is used to access property and then indexer.
        /// </param>
        /// <returns>Computed value and an error string.  If the string is non-null, then there was an evaluation error.</returns>
        public (T value, string error) TryEvaluate<T>(IMemory state)
        {
            var (result, error) = Evaluator.TryEvaluate(this, state);
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
                    return ((T)(object)Convert.ToBoolean(result), error);
                }

                if (typeof(T) == typeof(byte))
                {
                    return ((T)(object)Convert.ToByte(result), (Convert.ToByte(result) == Convert.ToDouble(result)) ? null : Error<T>(result));
                }

                if (typeof(T) == typeof(short))
                {
                    return ((T)(object)Convert.ToInt16(result), (Convert.ToInt16(result) == Convert.ToDouble(result)) ? null : Error<T>(result));
                }

                if (typeof(T) == typeof(int))
                {
                    return ((T)(object)Convert.ToInt32(result), (Convert.ToInt32(result) == Convert.ToDouble(result)) ? null : Error<T>(result));
                }

                if (typeof(T) == typeof(long))
                {
                    return ((T)(object)Convert.ToInt64(result), (Convert.ToInt64(result) == Convert.ToDouble(result)) ? null : Error<T>(result));
                }

                if (typeof(T) == typeof(ushort))
                {
                    return ((T)(object)Convert.ToUInt16(result), (Convert.ToUInt16(result) == Convert.ToDouble(result)) ? null : Error<T>(result));
                }

                if (typeof(T) == typeof(uint))
                {
                    return ((T)(object)Convert.ToUInt32(result), (Convert.ToUInt32(result) == Convert.ToDouble(result)) ? null : Error<T>(result));
                }

                if (typeof(T) == typeof(ulong))
                {
                    return ((T)(object)Convert.ToUInt64(result), (Convert.ToUInt64(result) == Convert.ToDouble(result)) ? null : Error<T>(result));
                }

                if (typeof(T) == typeof(float))
                {
                    return ((T)(object)Convert.ToSingle(Convert.ToDecimal(result)), null);
                }

                if (typeof(T) == typeof(double))
                {
                    return ((T)(object)Convert.ToDouble(Convert.ToDecimal(result)), null);
                }

                if (result == null)
                {
                    return (default(T), error);
                }

                return (JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(result)), null);
            }
            catch
            {
                return (default(T), Error<T>(result));
            }
        }

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
                var infix = Type.Length > 0 && !char.IsLetter(Type[0]) && Children.Count() >= 2;
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
        private class FunctionTable : IDictionary<string, ExpressionEvaluator>
        {
            private readonly ConcurrentDictionary<string, ExpressionEvaluator> customFunctions = new ConcurrentDictionary<string, ExpressionEvaluator>(StringComparer.InvariantCultureIgnoreCase);

            public ICollection<string> Keys => ExpressionFunctions.StandardFunctions.Keys.Concat(this.customFunctions.Keys).ToList();

            public ICollection<ExpressionEvaluator> Values => ExpressionFunctions.StandardFunctions.Values.Concat(this.customFunctions.Values).ToList();

            public int Count => ExpressionFunctions.StandardFunctions.Count + this.customFunctions.Count;

            public bool IsReadOnly => false;

            public ExpressionEvaluator this[string key]
            {
                get
                {
                    if (ExpressionFunctions.StandardFunctions.TryGetValue(key, out var function))
                    {
                        return function;
                    }

                    if (customFunctions.TryGetValue(key, out function))
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

                    customFunctions[key] = value;
                }
            }

            public void Add(string key, ExpressionEvaluator value) => this[key] = value;

            public void Add(KeyValuePair<string, ExpressionEvaluator> item) => this[item.Key] = item.Value;

            public void Clear() => this.customFunctions.Clear();

            public bool Contains(KeyValuePair<string, ExpressionEvaluator> item) => ExpressionFunctions.StandardFunctions.Contains(item) || this.customFunctions.Contains(item);

            public bool ContainsKey(string key) => ExpressionFunctions.StandardFunctions.ContainsKey(key) || this.customFunctions.ContainsKey(key);

            public void CopyTo(KeyValuePair<string, ExpressionEvaluator>[] array, int arrayIndex) => throw new NotImplementedException();

            public IEnumerator<KeyValuePair<string, ExpressionEvaluator>> GetEnumerator() => ExpressionFunctions.StandardFunctions.Concat(this.customFunctions).GetEnumerator();

            public bool Remove(string key) => this.customFunctions.TryRemove(key, out var oldVal);

            public bool Remove(KeyValuePair<string, ExpressionEvaluator> item) => Remove(item.Key);

            public bool TryGetValue(string key, out ExpressionEvaluator value)
            {
                if (ExpressionFunctions.StandardFunctions.TryGetValue(key, out value))
                {
                    return true;
                }

                if (this.customFunctions.TryGetValue(key, out value))
                {
                    return true;
                }

                return false;
            }

            IEnumerator IEnumerable.GetEnumerator() => ExpressionFunctions.StandardFunctions.Concat(this.customFunctions).GetEnumerator();
        }
    }
}
