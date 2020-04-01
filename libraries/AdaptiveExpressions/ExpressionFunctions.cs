// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using AdaptiveExpressions.Memory;
using AdaptiveExpressions.Properties;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions
{
    /// <summary>
    /// Definition of default built-in functions for expressions.
    /// </summary>
    /// <remarks>
    /// These functions are largely from WDL https://docs.microsoft.com/en-us/azure/logic-apps/workflow-definition-language-functions-reference
    /// with a few extensions like infix operators for math, logic and comparisons.
    ///
    /// This class also has some methods that are useful to use when defining custom functions.
    /// You can always construct a <see cref="ExpressionEvaluator"/> directly which gives the maximum amount of control over validation and evaluation.
    /// Validators are static checkers that should throw an exception if something is not valid statically.
    /// Evaluators are called to evaluate an expression and should try not to throw.
    /// There are some evaluators in this file that take in a verifier that is called at runtime to verify arguments are proper.
    /// </remarks>
    public static class ExpressionFunctions
    {
        /// <summary>
        /// Read only Dictionary of built in functions.
        /// </summary>
        public static readonly IDictionary<string, ExpressionEvaluator> StandardFunctions = GetStandardFunctions();

        /// <summary>
        /// Random number generator used for expressions.
        /// </summary>
        /// <remarks>This is exposed so that you can explicitly seed the random number generator for tests.</remarks>
        public static readonly Random Randomizer = new Random();

        /// <summary>
        /// The default date time format string.
        /// </summary>
        public static readonly string DefaultDateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";

        /// <summary>
        /// Object used to lock Randomizer.
        /// </summary>
        private static readonly object _randomizerLock = new object();

        /// <summary>
        /// Verify the result of an expression is of the appropriate type and return a string if not.
        /// </summary>
        /// <param name="value">Value to verify.</param>
        /// <param name="expression">Expression that produced value.</param>
        /// <param name="child">Index of child expression.</param>
        /// <returns>Null if value if correct or error string otherwise.</returns>
        public delegate string VerifyExpression(object value, Expression expression, int child);

        // Validators do static validation of expressions

        /// <summary>
        /// Validate that expression has a certain number of children that are of any of the supported types.
        /// </summary>
        /// <remarks>
        /// If a child has a return type of Object then validation will happen at runtime.</remarks>
        /// <param name="expression">Expression to validate.</param>
        /// <param name="minArity">Minimum number of children.</param>
        /// <param name="maxArity">Maximum number of children.</param>
        /// <param name="types">Allowed return types for children.</param>
        public static void ValidateArityAndAnyType(Expression expression, int minArity, int maxArity, params ReturnType[] types)
        {
            if (expression.Children.Length < minArity)
            {
                throw new ArgumentException($"{expression} should have at least {minArity} children.");
            }

            if (expression.Children.Length > maxArity)
            {
                throw new ArgumentException($"{expression} can't have more than {maxArity} children.");
            }

            if (types.Length > 0)
            {
                foreach (var child in expression.Children)
                {
                    if (child.ReturnType != ReturnType.Object && !types.Contains(child.ReturnType))
                    {
                        if (types.Count() == 1)
                        {
                            throw new ArgumentException($"{child} is not a {types[0]} expression in {expression}.");
                        }
                        else
                        {
                            var builder = new StringBuilder();
                            builder.Append($"{child} in {expression} is not any of [");
                            var first = true;
                            foreach (var type in types)
                            {
                                if (first)
                                {
                                    first = false;
                                }
                                else
                                {
                                    builder.Append(", ");
                                }

                                builder.Append(type);
                            }

                            builder.Append("].");
                            throw new ArgumentException(builder.ToString());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Validate the number and type of arguments to a function.
        /// </summary>
        /// <param name="expression">Expression to validate.</param>
        /// <param name="optional">Optional types in order.</param>
        /// <param name="types">Expected types in order.</param>
        public static void ValidateOrder(Expression expression, ReturnType[] optional, params ReturnType[] types)
        {
            if (optional == null)
            {
                optional = new ReturnType[0];
            }

            if (expression.Children.Length < types.Length || expression.Children.Length > types.Length + optional.Length)
            {
                throw new ArgumentException(optional.Length == 0
                    ? $"{expression} should have {types.Length} children."
                    : $"{expression} should have between {types.Length} and {types.Length + optional.Length} children.");
            }

            for (var i = 0; i < types.Length; ++i)
            {
                var child = expression.Children[i];
                var type = types[i];
                if (type != ReturnType.Object && child.ReturnType != ReturnType.Object && child.ReturnType != type)
                {
                    throw new ArgumentException($"{child} in {expression} is not a {type}.");
                }
            }

            for (var i = 0; i < optional.Length; ++i)
            {
                var ic = i + types.Length;
                if (ic >= expression.Children.Length)
                {
                    break;
                }

                var child = expression.Children[ic];
                var type = optional[i];
                if (type != ReturnType.Object && child.ReturnType != ReturnType.Object && child.ReturnType != type)
                {
                    throw new ArgumentException($"{child} in {expression} is not a {type}.");
                }
            }
        }

        /// <summary>
        /// Validate at least 1 argument of any type.
        /// </summary>
        /// <param name="expression">Expression to validate.</param>
        public static void ValidateAtLeastOne(Expression expression)
            => ValidateArityAndAnyType(expression, 1, int.MaxValue);

        /// <summary>
        /// Validate 1 or more numeric arguments.
        /// </summary>
        /// <param name="expression">Expression to validate.</param>
        public static void ValidateNumber(Expression expression)
            => ValidateArityAndAnyType(expression, 1, int.MaxValue, ReturnType.Number);

        /// <summary>
        /// Validate 1 or more string arguments.
        /// </summary>
        /// <param name="expression">Expression to validate.</param>
        public static void ValidateString(Expression expression)
            => ValidateArityAndAnyType(expression, 1, int.MaxValue, ReturnType.String);

        /// <summary>
        /// Validate there are two children.
        /// </summary>
        /// <param name="expression">Expression to validate.</param>
        public static void ValidateBinary(Expression expression)
            => ValidateArityAndAnyType(expression, 2, 2);

        /// <summary>
        /// Validate 2 numeric arguments.
        /// </summary>
        /// <param name="expression">Expression to validate.</param>
        public static void ValidateBinaryNumber(Expression expression)
            => ValidateArityAndAnyType(expression, 2, 2, ReturnType.Number);

        /// <summary>
        /// Validate 2 or more than 2 numeric arguments.
        /// </summary>
        /// <param name="expression">Expression to validate.</param>
        public static void ValidateTwoOrMoreThanTwoNumbers(Expression expression)
            => ValidateArityAndAnyType(expression, 2, int.MaxValue, ReturnType.Number);

        /// <summary>
        /// Validate there are 2 numeric or string arguments.
        /// </summary>
        /// <param name="expression">Expression to validate.</param>
        public static void ValidateBinaryNumberOrString(Expression expression)
            => ValidateArityAndAnyType(expression, 2, 2, ReturnType.Number, ReturnType.String);

        /// <summary>
        /// Validate there is a single argument.
        /// </summary>
        /// <param name="expression">Expression to validate.</param>
        public static void ValidateUnary(Expression expression)
            => ValidateArityAndAnyType(expression, 1, 1);

        /// <summary>
        /// Validate there is a single string argument.
        /// </summary>
        /// <param name="expression">Expression to validate.</param>
        public static void ValidateUnaryString(Expression expression)
            => ValidateArityAndAnyType(expression, 1, 1, ReturnType.String);

        /// <summary>
        /// Validate there is a single boolean argument.
        /// </summary>
        /// <param name="expression">Expression to validate.</param>
        public static void ValidateUnaryBoolean(Expression expression)
            => ValidateOrder(expression, null, ReturnType.Boolean);

        // Verifiers do runtime error checking of expression evaluation.

        /// <summary>
        /// Verify value is numeric.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="expression">Expression that led to value.</param>
        /// <param name="number">No function.</param>
        /// <returns>Error or null if valid.</returns>
        public static string VerifyNumber(object value, Expression expression, int number)
        {
            string error = null;
            if (!value.IsNumber())
            {
                error = $"{expression} is not a number.";
            }

            return error;
        }

        /// <summary>
        /// Verify value is numeric list.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="expression">Expression that led to value.</param>
        /// <param name="number">No function.</param>
        /// <returns>Error or null if valid.</returns>
        public static string VerifyNumericList(object value, Expression expression, int number)
        {
            string error = null;
            if (!TryParseList(value, out var list))
            {
                error = $"{expression} is not a list.";
            }
            else
            {
                foreach (var elt in list)
                {
                    if (!elt.IsNumber())
                    {
                        error = $"{elt} is not a number in {expression}";
                        break;
                    }
                }
            }

            return error;
        }

        /// <summary>
        /// Verify value is a numeric list or a numeric value.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="expression">Expression that led to value.</param>
        /// <param name="number">No function.</param>
        /// <returns>Error or null if valid.</returns>
        public static string VerifyNumericListOrNumber(object value, Expression expression, int number)
        {
            string error = null;
            if (value.IsNumber())
            {
                return error;
            }

            if (!TryParseList(value, out var list))
            {
                error = $"{expression} is neither a list nor a number.";
            }
            else
            {
                foreach (var elt in list)
                {
                    if (!elt.IsNumber())
                    {
                        error = $"{elt} is not a number in {expression}";
                        break;
                    }
                }
            }

            return error;
        }

        /// <summary>
        /// Verify value contains elements.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="expression">Expression that led to value.</param>
        /// <param name="number">No function.</param>
        /// <returns>Error or null if valid.</returns>
        public static string VerifyContainer(object value, Expression expression, int number)
        {
            string error = null;
            if (!(value is string) && !(value is IList) && !(value is IEnumerable))
            {
                error = $"{expression} must be a string or list.";
            }

            return error;
        }

        /// <summary>
        /// Verify value contains elements.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="expression">Expression that led to value.</param>
        /// <param name="number">No function.</param>
        /// <returns>Error or null if valid.</returns>
        public static string VerifyList(object value, Expression expression, int number)
        {
            string error = null;
            if (!TryParseList(value, out var _))
            {
                error = $"{expression} must be a list.";
            }

            return error;
        }

        /// <summary>
        /// Try to coerce object to IList.
        /// </summary>
        /// <param name="value">Value to coerce.</param>
        /// <param name="list">IList if found.</param>
        /// <returns>true if found IList.</returns>
        public static bool TryParseList(object value, out IList list)
        {
            var isList = false;
            list = null;
            if (!(value is JObject) && value is IList listValue)
            {
                list = listValue;
                isList = true;
            }

            return isList;
        }

        /// <summary>
        /// Verify value is an integer.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="expression">Expression that led to value.</param>
        /// <param name="number">No function.</param>
        /// <returns>Error or null if valid.</returns>
        public static string VerifyInteger(object value, Expression expression, int number)
        {
            string error = null;
            if (!value.IsInteger())
            {
                error = $"{expression} is not an integer.";
            }

            return error;
        }

        /// <summary>
        /// Verify value is a string.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="expression">Expression that led to value.</param>
        /// <param name="number">No function.</param>
        /// <returns>Error or null if valid.</returns>
        public static string VerifyString(object value, Expression expression, int number)
        {
            string error = null;
            if (!(value is string))
            {
                error = $"{expression} is not a string.";
            }

            return error;
        }

        /// <summary>
        /// Verify an object is neither a string nor null.
        /// </summary>
        /// <param name="value">instance.</param>
        /// <param name="expression">expression.</param>
        /// <param name="number">number.</param>
        /// <returns>error message.</returns>
        public static string VerifyStringOrNull(object value, Expression expression, int number)
        {
            string error = null;
            if (!(value is string) && value != null)
            {
                error = $"{expression} is neither a string nor a null object.";
            }

            return error;
        }

        /// <summary>
        /// Verify value is not null.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="expression">Expression that led to value.</param>
        /// <param name="number">No function.</param>
        /// <returns>Error or null if valid.</returns>
        public static string VerifyNotNull(object value, Expression expression, int number)
        {
            string error = null;
            if (value == null)
            {
                error = $"{expression} is null.";
            }

            return error;
        }

        /// <summary>
        /// Verify value is a number or string.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="expression">Expression that led to value.</param>
        /// <param name="number">No function.</param>
        /// <returns>Error or null if valid.</returns>
        public static string VerifyNumberOrString(object value, Expression expression, int number)
        {
            string error = null;
            if (value == null || (!value.IsNumber() && !(value is string)))
            {
                error = $"{expression} is not string or number.";
            }

            return error;
        }

        /// <summary>
        /// Verify value is a number or string or null.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="expression">Expression that led to value.</param>
        /// <param name="number">No function.</param>
        /// <returns>Error.</returns>
        public static string VerifyNumberOrStringOrNull(object value, Expression expression, int number)
        {
            string error = null;
            if (value != null && !value.IsNumber() && !(value is string))
            {
                error = $"{expression} is not string or number.";
            }

            return error;
        }

        /// <summary>
        /// Verify value is boolean.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="expression">Expression that led to value.</param>
        /// <returns>Error or null if valid.</returns>
        public static string VerifyBoolean(object value, Expression expression)
        {
            string error = null;
            if (!(value is bool))
            {
                error = $"{expression} is not a boolean.";
            }

            return error;
        }

        /// <summary>
        /// Evaluate expression children and return them.
        /// </summary>
        /// <param name="expression">Expression with children.</param>
        /// <param name="state">Global state.</param>
        /// <param name="verify">Optional function to verify each child's result.</param>
        /// <returns>List of child values or error message.</returns>
        public static (IReadOnlyList<object>, string error) EvaluateChildren(Expression expression, IMemory state, VerifyExpression verify = null)
        {
            var args = new List<object>();
            object value;
            string error = null;
            var pos = 0;
            foreach (var child in expression.Children)
            {
                (value, error) = child.TryEvaluate(state);
                if (error != null)
                {
                    break;
                }

                if (verify != null)
                {
                    error = verify(value, child, pos);
                }

                if (error != null)
                {
                    break;
                }

                args.Add(value);
                ++pos;
            }

            return (args, error);
        }

        // Apply -- these are helpers for adding functions to the expression library.

        /// <summary>
        /// Generate an expression delegate that applies function after verifying all children.
        /// </summary>
        /// <param name="function">Function to apply.</param>
        /// <param name="verify">Function to check each arg for validity.</param>
        /// <returns>Delegate for evaluating an expression.</returns>
        public static EvaluateExpressionDelegate Apply(Func<IReadOnlyList<object>, object> function, VerifyExpression verify = null)
            =>
            (expression, state) =>
            {
                object value = null;
                string error = null;
                IReadOnlyList<object> args;
                (args, error) = EvaluateChildren(expression, state, verify);
                if (error == null)
                {
                    try
                    {
                        value = function(args);
                    }
                    catch (Exception e)
                    {
                        error = e.Message;
                    }
                }

                value = ResolveValue(value);

                return (value, error);
            };

        /// <summary>
        /// Generate an expression delegate that applies function after verifying all children.
        /// </summary>
        /// <param name="function">Function to apply.</param>
        /// <param name="verify">Function to check each arg for validity.</param>
        /// <returns>Delegate for evaluating an expression.</returns>
        public static EvaluateExpressionDelegate ApplyWithError(Func<IReadOnlyList<object>, (object, string)> function, VerifyExpression verify = null)
            =>
            (expression, state) =>
            {
                object value = null;
                string error = null;
                IReadOnlyList<object> args;
                (args, error) = EvaluateChildren(expression, state, verify);
                if (error == null)
                {
                    try
                    {
                        (value, error) = function(args);
                    }
                    catch (Exception e)
                    {
                        error = e.Message;
                    }
                }

                value = ResolveValue(value);

                return (value, error);
            };

        /// <summary>
        /// Generate an expression delegate that applies function on the accumulated value after verifying all children.
        /// </summary>
        /// <param name="function">Function to apply.</param>
        /// <param name="verify">Function to check each arg for validity.</param>
        /// <returns>Delegate for evaluating an expression.</returns>
        public static EvaluateExpressionDelegate ApplySequence(Func<IReadOnlyList<object>, object> function, VerifyExpression verify = null)
            => Apply(
                args =>
                {
                    var binaryArgs = new List<object> { null, null };
                    var sofar = args[0];
                    for (var i = 1; i < args.Count; ++i)
                    {
                        binaryArgs[0] = sofar;
                        binaryArgs[1] = args[i];
                        sofar = function(binaryArgs);
                    }

                    return sofar;
                }, verify);

        /// <summary>
        /// Generate an expression delegate that applies function on the accumulated value after verifying all children.
        /// </summary>
        /// <param name="function">Function to apply.</param>
        /// <param name="verify">Function to check each arg for validity.</param>
        /// <returns>Delegate for evaluating an expression.</returns>
        public static EvaluateExpressionDelegate ApplySequenceWithError(Func<IReadOnlyList<object>, (object, string)> function, VerifyExpression verify = null)
            => ApplyWithError(
                args =>
                {
                    var binaryArgs = new List<object> { null, null };
                    var sofar = args[0];
                    for (var i = 1; i < args.Count; ++i)
                    {
                        binaryArgs[0] = sofar;
                        binaryArgs[1] = args[i];
                        var (result, error) = function(binaryArgs);
                        if (error != null)
                        {
                            return (result, error);
                        }
                        else
                        {
                            sofar = result;
                        }
                    }

                    return (sofar, null);
                }, verify);

        /// <summary>
        /// Numeric operators that can have 1 or more args.
        /// </summary>
        /// <param name="type">Expression type.</param>
        /// <param name="function">Function to apply.</param>
        /// <returns>Delegate for evaluating an expression.</returns>
        public static ExpressionEvaluator Numeric(string type, Func<IReadOnlyList<object>, object> function)
            => new ExpressionEvaluator(type, ApplySequence(function, VerifyNumber), ReturnType.Number, ValidateNumber);

        /// <summary>
        /// Numeric or Collection operators that can have 1 or more args. It can be apply numeric values or a collection of numeric
        /// values, or a mixing of  numeric values and a collection.
        /// </summary>
        /// <param name="type">Expression type.</param>
        /// <param name="function">Function to apply.</param>
        /// <returns>Delegate for evaluating an expression.</returns>
        public static ExpressionEvaluator NumericOrCollection(string type, Func<IReadOnlyList<object>, object> function)
            => new ExpressionEvaluator(type, Apply(function, VerifyNumericListOrNumber), ReturnType.Number, ValidateAtLeastOne);

        /// <summary>
        /// Numeric operators that can have 2 or more args.
        /// </summary>
        /// <param name="type">Expression type.</param>
        /// <param name="function">Function to apply.</param>
        /// <param name="verify">Function to verify arguments.</param>
        /// <returns>Delegate for evaluating an expression.</returns>
        public static ExpressionEvaluator MultivariateNumeric(string type, Func<IReadOnlyList<object>, object> function, VerifyExpression verify = null)
            => new ExpressionEvaluator(type, ApplySequence(function, verify ?? VerifyNumber), ReturnType.Number, ValidateTwoOrMoreThanTwoNumbers);

        /// <summary>
        /// Comparison operators.
        /// </summary>
        /// <remarks>
        /// A comparison operator returns false if the comparison is false, or there is an error.  This prevents errors from short-circuiting boolean expressions.
        /// </remarks>
        /// <param name="type">Expression type.</param>
        /// <param name="function">Function to apply.</param>
        /// <param name="validator">Function to validate expression.</param>
        /// <param name="verify">Function to verify arguments to expression.</param>
        /// <returns>Delegate for evaluating an expression.</returns>
        public static ExpressionEvaluator Comparison(
            string type,
            Func<IReadOnlyList<object>, bool> function,
            ValidateExpressionDelegate validator,
            VerifyExpression verify = null)
            => new ExpressionEvaluator(
                type,
                (expression, state) =>
                {
                    var result = false;
                    string error = null;
                    IReadOnlyList<object> args;
                    (args, error) = EvaluateChildren(expression, state, verify);
                    if (error == null)
                    {
                        // Ensure args are all of same type
                        bool? isNumber = null;
                        foreach (var arg in args)
                        {
                            var obj = arg;
                            if (isNumber.HasValue)
                            {
                                if (obj != null && obj.IsNumber() != isNumber.Value)
                                {
                                    error = $"Arguments must either all be numbers or strings in {expression}";
                                    break;
                                }
                            }
                            else
                            {
                                isNumber = obj.IsNumber();
                            }
                        }

                        if (error == null)
                        {
                            try
                            {
                                result = function(args);
                            }
                            catch (Exception e)
                            {
                                // NOTE: This should not happen in normal execution
                                error = e.Message;
                            }
                        }
                    }
                    else
                    {
                        // Swallow errors and treat as false
                        error = null;
                    }

                    return (result, error);
                },
                ReturnType.Boolean,
                validator);

        /// <summary>
        /// Transform a string into another string.
        /// </summary>
        /// <param name="type">Expression type.</param>
        /// <param name="function">Function to apply.</param>
        /// <returns>Delegate for evaluating an expression.</returns>
        public static ExpressionEvaluator StringTransform(string type, Func<IReadOnlyList<object>, object> function)
            => new ExpressionEvaluator(type, Apply(function, VerifyStringOrNull), ReturnType.String, ValidateUnaryString);

        /// <summary>
        /// Transform a date-time to another date-time.
        /// </summary>
        /// <param name="type">Expression type.</param>
        /// <param name="function">Transformer.</param>
        /// <returns>Delegate for evaluating expression.</returns>
        public static ExpressionEvaluator TimeTransform(string type, Func<DateTime, int, DateTime> function)
            => new ExpressionEvaluator(
                type,
                (expr, state) =>
                {
                    object value = null;
                    string error = null;
                    IReadOnlyList<object> args;
                    (args, error) = EvaluateChildren(expr, state);
                    if (error == null)
                    {
                        if (args[0] is string string0 && args[1].IsInteger())
                        {
                            var formatString = (args.Count() == 3 && args[2] is string string1) ? string1 : DefaultDateTimeFormat;
                            (value, error) = ParseISOTimestamp(string0, dt => function(dt, Convert.ToInt32(args[1])).ToString(formatString));
                        }
                        else
                        {
                            error = $"{expr} could not be evaluated";
                        }
                    }

                    return (value, error);
                },
                ReturnType.String,
                expr => ValidateArityAndAnyType(expr, 2, 3, ReturnType.String, ReturnType.Number));

        /// <summary>
        /// Lookup an index property of instance.
        /// </summary>
        /// <param name="instance">Instance with property.</param>
        /// <param name="index">Property to lookup.</param>
        /// <returns>Value and error information if any.</returns>
        public static (object value, string error) AccessIndex(object instance, int index)
        {
            // NOTE: This returns null rather than an error if property is not present
            if (instance == null)
            {
                return (null, null);
            }

            object value = null;
            string error = null;

            if (TryParseList(instance, out var list))
            {
                if (index >= 0 && index < list.Count)
                {
                    value = list[index];
                }
                else
                {
                    error = $"Index was out of range.";
                }
            }
            else
            {
                error = $"{instance} is not a collection.";
            }

            value = ResolveValue(value);

            return (value, error);
        }

        /// <summary>
        /// Lookup a property in IDictionary, JObject or through reflection.
        /// </summary>
        /// <param name="instance">Instance with property.</param>
        /// <param name="property">Property to lookup.</param>
        /// <param name="value">Value of property.</param>
        /// <returns>True if property is present and binds value.</returns>
        public static bool TryAccessProperty(object instance, string property, out object value)
        {
            var isPresent = false;
            value = null;
            if (instance != null)
            {
                property = property.ToLower();

                // NOTE: what about other type of TKey, TValue?
                if (instance is IDictionary<string, object> idict)
                {
                    if (!idict.TryGetValue(property, out value))
                    {
                        // fall back to case insensitive
                        var prop = idict.Keys.Where(k => k.ToLower() == property).SingleOrDefault();
                        if (prop != null)
                        {
                            isPresent = idict.TryGetValue(prop, out value);
                        }
                    }
                    else
                    {
                        isPresent = true;
                    }
                }
                else if (instance is JObject jobj)
                {
                    value = jobj.GetValue(property, StringComparison.CurrentCultureIgnoreCase);
                    isPresent = value != null;
                }
                else
                {
                    // Use reflection
                    var type = instance.GetType();
                    var prop = type.GetProperties().Where(p => p.Name.ToLower() == property).SingleOrDefault();
                    if (prop != null)
                    {
                        value = prop.GetValue(instance);
                        isPresent = true;
                    }
                }

                if (isPresent)
                {
                    value = ResolveValue(value);
                }
            }

            return isPresent;
        }

        /// <summary>
        /// Set the property into a given instance.
        /// </summary>
        /// <param name="instance">Given instance.</param>
        /// <param name="property">Property be set.</param>
        /// <param name="value">Value be set.</param>
        /// <returns>Value and error information if any.</returns>
        public static (object result, string error) SetProperty(object instance, string property, object value)
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
                if (value != null)
                {
                    result = JToken.FromObject(value);
                    jobj[property] = (JToken)result;
                }
                else
                {
                    jobj[property] = null;
                }
            }
            else
            {
                // Use reflection
                var type = instance.GetType();
                var prop = type.GetProperties().Where(p => p.Name.ToLower() == property).SingleOrDefault();
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

        /// <summary>
        /// Convert constant JValue to base type value.
        /// </summary>
        /// <param name="obj">input object.</param>
        /// <returns>Corresponding base type if input is a JValue.</returns>
        public static object ResolveValue(object obj)
        {
            object value;
            if (!(obj is JValue jval))
            {
                value = obj;
            }
            else
            {
                value = jval.Value;
                if (jval.Type == JTokenType.Integer)
                {
                    value = jval.ToObject<int>();
                }
                else if (jval.Type == JTokenType.String)
                {
                    value = jval.ToObject<string>();
                }
                else if (jval.Type == JTokenType.Boolean)
                {
                    value = jval.ToObject<bool>();
                }
                else if (jval.Type == JTokenType.Float)
                {
                    value = jval.ToObject<float>();
                }
            }

            return value;
        }

        /// <summary>
        /// Try to accumulate the path from an Accessor or Element, from right to left.
        /// </summary>
        /// <param name="expression">expression.</param>
        /// <param name="state">scope.</param>
        /// <returns>return the accumulated path and the expression left unable to accumulate.</returns>
        public static (string path, Expression left, string error) TryAccumulatePath(Expression expression, IMemory state)
        {
            var path = string.Empty;
            var left = expression;

            // get path from Accessor or Element+Accessor
            while (left != null)
            {
                if (left.Type == ExpressionType.Accessor)
                {
                    path = (string)((Constant)left.Children[0]).Value + "." + path;
                    left = left.Children.Length == 2 ? left.Children[1] : null;
                }
                else if (left.Type == ExpressionType.Element)
                {
                    var (value, error) = left.Children[1].TryEvaluate(state);
                    if (error != null)
                    {
                        return (null, null, error);
                    }

                    if (value is int)
                    {
                        path = $"[{value}]" + "." + path;
                    }
                    else if (value is string)
                    {
                        path = $"['{value}']" + "." + path;
                    }
                    else
                    {
                        return (null, null, $"{left.Children[1].ToString()} doesn't return an int or string");
                    }

                    left = left.Children[0];
                }
                else
                {
                    break;
                }
            }

            // make sure we generated a valid path
            path = path.TrimEnd('.').Replace(".[", "[");

            if (string.IsNullOrEmpty(path))
            {
                path = null;
            }

            return (path, left, null);
        }

        private static void ValidateAccessor(Expression expression)
        {
            var children = expression.Children;
            if (children.Length == 0
                || !(children[0] is Constant cnst)
                || cnst.ReturnType != ReturnType.String)
            {
                throw new Exception($"{expression} must have a string as first argument.");
            }

            if (children.Length > 2)
            {
                throw new Exception($"{expression} has more than 2 children.");
            }

            if (children.Length == 2 && children[1].ReturnType != ReturnType.Object)
            {
                throw new Exception($"{expression} must have an object as its second argument.");
            }
        }

        private static (object value, string error) Accessor(Expression expression, IMemory state)
        {
            var (path, left, error) = TryAccumulatePath(expression, state);

            if (error != null)
            {
                return (null, error);
            }

            if (left == null)
            {
                // fully converted to path, so we just delegate to memory scope
                return WrapGetValue(state, path);
            }
            else
            {
                // stop at somewhere, so we figure out what's left
                var (newScope, err) = left.TryEvaluate(state);
                if (err != null)
                {
                    return (null, err);
                }

                return WrapGetValue(MemoryFactory.Create(newScope), path);
            }
        }

        private static (object value, string error) GetProperty(Expression expression, IMemory state)
        {
            object value = null;
            string error;
            object instance;
            object property;

            var children = expression.Children;
            (instance, error) = children[0].TryEvaluate(state);
            if (error == null)
            {
                (property, error) = children[1].TryEvaluate(state);
                if (error == null)
                {
                    (value, error) = WrapGetValue(MemoryFactory.Create(instance), (string)property);
                }
            }

            return (value, error);
        }

        private static (object value, string error) WrapGetValue(IMemory memory, string property)
        {
            if (memory.TryGetValue(property, out var result))
            {
                return (result, null);
            }

            return (null, null);
        }

        private static (object value, string error) ExtractElement(Expression expression, IMemory state)
        {
            object value = null;
            string error;
            var instance = expression.Children[0];
            var index = expression.Children[1];
            object inst;
            (inst, error) = instance.TryEvaluate(state);
            if (error == null)
            {
                object idxValue;
                (idxValue, error) = index.TryEvaluate(state);
                if (error == null)
                {
                    if (idxValue is int idx)
                    {
                        (value, error) = AccessIndex(inst, idx);
                    }
                    else if (idxValue is string idxStr)
                    {
                        TryAccessProperty(inst, idxStr, out value);
                    }
                    else
                    {
                        error = $"Could not coerce {index}<{idxValue.GetType()}> to an int or string";
                    }
                }
            }

            return (value, error);
        }

        private static (object value, string error) SetPathToValue(Expression expr, IMemory state)
        {
            var (path, left, error) = TryAccumulatePath(expr.Children[0], state);

            if (error != null)
            {
                return (null, error);
            }

            if (left != null)
            {
                // the expression can't be fully merged as a path
                return (null, $"{expr.Children[0].ToString()} is not a valid path to set value");
            }

            var (value, err) = expr.Children[1].TryEvaluate(state);
            if (err != null)
            {
                return (null, err);
            }

            state.SetValue(path, value);
            return (value, null);
        }

        private static string ParseStringOrNull(object value)
        {
            string result;
            if (value is string str)
            {
                result = str;
            }
            else
            {
                result = string.Empty;
            }

            return result;
        }

        /// <summary>
        /// Return new object list replace jarray.ToArray&lt;object&gt;().
        /// </summary>
        /// <param name="instance">List to resolve.</param>
        /// <returns>Resolved list.</returns>
        private static IList ResolveListValue(object instance)
        {
            IList result = null;
            if (instance is JArray ja)
            {
                result = (IList)ja.ToObject(typeof(List<object>));
            }
            else if (TryParseList(instance, out var list))
            {
                result = list;
            }

            return result;
        }

        private static bool IsEmpty(object instance)
        {
            bool result;
            if (instance == null)
            {
                result = true;
            }
            else if (instance is string string0)
            {
                result = string.IsNullOrEmpty(string0);
            }
            else if (TryParseList(instance, out var list))
            {
                result = list.Count == 0;
            }
            else
            {
                result = instance.GetType().GetProperties().Length == 0;
            }

            return result;
        }

        /// <summary>
        /// Test result to see if True in logical comparison functions.
        /// </summary>
        /// <param name="instance">Computed value.</param>
        /// <returns>True if boolean true or non-null.</returns>
        private static bool IsLogicTrue(object instance)
        {
            var result = true;
            if (instance is bool instanceBool)
            {
                result = instanceBool;
            }
            else if (instance == null)
            {
                result = false;
            }

            return result;
        }

        private static object Max(object a, object b)
        {
            if (a == null || b == null)
            {
                throw new ArgumentNullException();
            }

            if (Convert.ToDouble(a) > Convert.ToDouble(b))
            {
                return a;
            }
            else
            {
                return b;
            }
        }

        private static object Min(object a, object b)
        {
            if (a == null || b == null)
            {
                throw new ArgumentNullException();
            }

            if (Convert.ToDouble(a) <= Convert.ToDouble(b))
            {
                return a;
            }
            else
            {
                return b;
            }
        }

        private static object Add(object a, object b)
        {
            if (a == null || b == null)
            {
                throw new ArgumentNullException();
            }

            if (a.IsInteger() && b.IsInteger())
            {
                return Convert.ToInt32(a) + Convert.ToInt32(b);
            }
            else
            {
                return Convert.ToDouble(a) + Convert.ToDouble(b);
            }
        }

        private static object Subtract(object a, object b)
        {
            if (a == null || b == null)
            {
                throw new ArgumentNullException();
            }

            if (a.IsInteger() && b.IsInteger())
            {
                return Convert.ToInt32(a) - Convert.ToInt32(b);
            }
            else
            {
                return Convert.ToDouble(a) - Convert.ToDouble(b);
            }
        }

        private static object Multiply(object a, object b)
        {
            if (a == null || b == null)
            {
                throw new ArgumentNullException();
            }

            if (a.IsInteger() && b.IsInteger())
            {
                return Convert.ToInt32(a) * Convert.ToInt32(b);
            }
            else
            {
                return Convert.ToDouble(a) * Convert.ToDouble(b);
            }
        }

        private static object Mod(object a, object b)
        {
            if (a == null || b == null)
            {
                throw new ArgumentNullException();
            }

            if (a.IsInteger() && b.IsInteger())
            {
                return Convert.ToInt32(a) % Convert.ToInt32(b);
            }
            else
            {
                return Convert.ToDouble(a) % Convert.ToDouble(b);
            }
        }

        private static object Divide(object a, object b)
        {
            if (a == null || b == null)
            {
                throw new ArgumentNullException();
            }

            if (a.IsInteger() && b.IsInteger())
            {
                return Convert.ToInt32(a) / Convert.ToInt32(b);
            }
            else
            {
                return Convert.ToDouble(a) / Convert.ToDouble(b);
            }
        }

        private static (object value, string error) And(Expression expression, IMemory state)
        {
            object result = true;
            string error = null;
            foreach (var child in expression.Children)
            {
                (result, error) = child.TryEvaluate(state);
                if (error == null)
                {
                    if (IsLogicTrue(result))
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                        break;
                    }
                }
                else
                {
                    // We interpret any error as false and swallow the error
                    result = false;
                    error = null;
                    break;
                }
            }

            return (result, error);
        }

        private static (object value, string error) Or(Expression expression, IMemory state)
        {
            object result = false;
            string error = null;
            foreach (var child in expression.Children)
            {
                (result, error) = child.TryEvaluate(state);
                if (error == null)
                {
                    if (IsLogicTrue(result))
                    {
                        result = true;
                        break;
                    }
                }
                else
                {
                    // Interpret error as false and swallow errors
                    error = null;
                }
            }

            return (result, error);
        }

        private static (object value, string error) Not(Expression expression, IMemory state)
        {
            object result;
            string error;
            (result, error) = expression.Children[0].TryEvaluate(state);
            if (error == null)
            {
                result = !IsLogicTrue(result);
            }
            else
            {
                error = null;
                result = true;
            }

            return (result, error);
        }

        private static (object value, string error) If(Expression expression, IMemory state)
        {
            object result;
            string error;
            (result, error) = expression.Children[0].TryEvaluate(state);
            if (error == null && IsLogicTrue(result))
            {
                (result, error) = expression.Children[1].TryEvaluate(state);
            }
            else
            {
                // Swallow error and treat as false
                (result, error) = expression.Children[2].TryEvaluate(state);
            }

            return (result, error);
        }

        private static (object value, string error) Substring(Expression expression, IMemory state)
        {
            string result = null;
            string error;
            string str;
            (str, error) = expression.Children[0].TryEvaluate<string>(state);
            if (error == null)
            {
                if (str == null)
                {
                    result = string.Empty;
                }
                else
                {
                    int start;
                    var startExpr = expression.Children[1];
                    (start, error) = startExpr.TryEvaluate<int>(state);
                    if (error == null && (start < 0 || start >= str.Length))
                    {
                        error = $"{startExpr}={start} which is out of range for {str}.";
                    }

                    if (error == null)
                    {
                        int length;
                        if (expression.Children.Length == 2)
                        {
                            // Without length, compute to end
                            length = str.Length - start;
                        }
                        else
                        {
                            var lengthExpr = expression.Children[2];
                            (length, error) = lengthExpr.TryEvaluate<int>(state);
                            if (error == null && (length < 0 || start + length > str.Length))
                            {
                                error = $"{lengthExpr}={length} which is out of range for {str}.";
                            }
                        }

                        if (error == null)
                        {
                            result = str.Substring(start, length);
                        }
                    }
                }
            }

            return (result, error);
        }

        private static (object value, string error) Foreach(Expression expression, IMemory state)
        {
            object result = null;
            string error;

            object instance;
            (instance, error) = expression.Children[0].TryEvaluate(state);
            if (instance == null)
            {
                error = $"'{expression.Children[0]}' evaluated to null.";
            }
            else if (error == null)
            {
                IList list = null;
                if (TryParseList(instance, out IList ilist))
                {
                    list = ilist;
                }
                else if (instance is JObject jobj)
                {
                    list = Object2KVPairList(jobj);
                }
                else if (JToken.FromObject(instance) is JObject jobject)
                {
                    list = Object2KVPairList(jobject);
                }
                else
                {
                    error = $"{expression.Children[0]} is not a collection or structure object to run foreach";
                }

                if (error == null)
                {
                    var iteratorName = (string)(expression.Children[1].Children[0] as Constant).Value;
                    var stackedMemory = StackedMemory.Wrap(state);
                    result = new List<object>();
                    for (var idx = 0; idx < list.Count; idx++)
                    {
                        var local = new Dictionary<string, object>
                        {
                            { iteratorName, AccessIndex(list, idx).value },
                        };

                        // the local iterator is pushed as one memory layer in the memory stack
                        stackedMemory.Push(new SimpleObjectMemory(local));
                        (var r, var e) = expression.Children[2].TryEvaluate(stackedMemory);
                        stackedMemory.Pop();

                        if (e != null)
                        {
                            return (null, e);
                        }

                        ((List<object>)result).Add(r);
                    }
                }
            }

            return (result, error);
        }

        private static (object value, string error) Where(Expression expression, IMemory state)
        {
            object result = null;
            string error;

            object instance;
            (instance, error) = expression.Children[0].TryEvaluate(state);
            if (error == null)
            {
                var isInstanceList = false;
                IList list = null;
                if (TryParseList(instance, out IList ilist))
                {
                    isInstanceList = true;
                    list = ilist;
                }
                else if (instance is JObject jobj)
                {
                    list = Object2KVPairList(jobj);
                }
                else if (JToken.FromObject(instance) is JObject jobject)
                {
                    list = Object2KVPairList(jobject);
                }
                else
                {
                    error = $"{expression.Children[0]} is not a collection or structure object to run foreach";
                }

                if (error == null)
                {
                    var iteratorName = (string)(expression.Children[1].Children[0] as Constant).Value;
                    var stackedMemory = StackedMemory.Wrap(state);
                    result = new List<object>();
                    for (var idx = 0; idx < list.Count; idx++)
                    {
                        var local = new Dictionary<string, object>
                        {
                            { iteratorName, AccessIndex(list, idx).value },
                        };

                        // the local iterator is pushed as one memory layer in the memory stack
                        stackedMemory.Push(new SimpleObjectMemory(local));
                        var (r, _) = expression.Children[2].TryEvaluate<bool>(stackedMemory);
                        stackedMemory.Pop();

                        if (r)
                        {
                            // add if only if it evaluates to true
                            ((List<object>)result).Add(local[iteratorName]);
                        }
                    }

                    if (!isInstanceList)
                    {
                        // re-construct object
                        var jobjResult = new JObject();
                        foreach (var item in (List<object>)result)
                        {
                            TryAccessProperty(item, "key", out var keyVal);
                            TryAccessProperty(item, "value", out var val);
                            jobjResult.Add(keyVal as string, JToken.FromObject(val));
                        }

                        result = jobjResult;
                    }
                }
            }

            return (result, error);
        }

        private static List<object> Object2KVPairList(JObject jobj)
        {
            var tempList = new List<object>();
            foreach (var item in jobj)
            {
                tempList.Add(new { key = item.Key, value = item.Value });
            }

            return tempList;
        }

        private static void ValidateWhere(Expression expression) => ValidateForeach(expression);

        private static void ValidateForeach(Expression expression)
        {
            if (expression.Children.Count() != 3)
            {
                throw new Exception($"foreach expect 3 parameters, found {expression.Children.Count()}");
            }

            var second = expression.Children[1];

            if (!(second.Type == ExpressionType.Accessor && second.Children.Count() == 1))
            {
                throw new Exception($"Second parameter of foreach is not an identifier : {second}");
            }
        }

        private static void ValidateIsMatch(Expression expression)
        {
            ValidateArityAndAnyType(expression, 2, 2, ReturnType.String);

            var second = expression.Children[1];
            if (second.ReturnType == ReturnType.String && second.Type == ExpressionType.Constant)
            {
                CommonRegex.CreateRegex((second as Constant).Value.ToString());
            }
        }

        private static (Func<DateTime, DateTime>, string) DateTimeConverter(long interval, string timeUnit, bool isPast = true)
        {
            Func<DateTime, DateTime> converter = (dateTime) => dateTime;
            string error = null;
            var multiFlag = isPast ? -1 : 1;
            switch (timeUnit.ToLower())
            {
                case "second": converter = (dateTime) => dateTime.AddSeconds(multiFlag * interval); break;
                case "minute": converter = (dateTime) => dateTime.AddMinutes(multiFlag * interval); break;
                case "hour": converter = (dateTime) => dateTime.AddHours(multiFlag * interval); break;
                case "day": converter = (dateTime) => dateTime.AddDays(multiFlag * interval); break;
                case "week": converter = (dateTime) => dateTime.AddDays(multiFlag * (interval * 7)); break;
                case "month": converter = (dateTime) => dateTime.AddMonths(multiFlag * Convert.ToInt32(interval)); break;
                case "year": converter = (dateTime) => dateTime.AddYears(multiFlag * Convert.ToInt32(interval)); break;
                default: error = $"{timeUnit} is not a valid time unit."; break;
            }

            return (converter, error);
        }

        private static (object, string) ParseTimestamp(string timeStamp, Func<DateTime, object> transform = null)
        {
            object result = null;
            string error = null;
            if (DateTime.TryParse(
                    s: timeStamp,
                    provider: CultureInfo.InvariantCulture,
                    styles: DateTimeStyles.RoundtripKind,
                    result: out var parsed))
            {
                result = transform != null ? transform(parsed) : parsed;
            }
            else
            {
                error = $"Could not parse {timeStamp}";
            }

            return (result, error);
        }

        private static (object, string) ParseISOTimestamp(string timeStamp, Func<DateTime, object> transform = null)
        {
            object result = null;
            string error = null;
            if (DateTime.TryParse(
                    s: timeStamp,
                    provider: CultureInfo.InvariantCulture,
                    styles: DateTimeStyles.RoundtripKind,
                    result: out var parsed))
            {
                if (parsed.ToString(DefaultDateTimeFormat).Equals(timeStamp, StringComparison.InvariantCultureIgnoreCase))
                {
                    result = transform != null ? transform(parsed) : parsed;
                }
                else
                {
                    error = $"{timeStamp} is not standard ISO format.";
                }
            }
            else
            {
                error = $"Could not parse {timeStamp}";
            }

            return (result, error);
        }

        private static (object, string) ConvertTimeZoneFormat(string timezone)
        {
            object convertedTimeZone = null;
            string convertedTimeZoneStr;
            string error = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                convertedTimeZoneStr = TimeZoneConverter.IanaToWindows(timezone);
            }
            else
            {
                convertedTimeZoneStr = TimeZoneConverter.WindowsToIana(timezone);
            }

            try
            {
                convertedTimeZone = TimeZoneInfo.FindSystemTimeZoneById(convertedTimeZoneStr);
            }
            catch
            {
                error = $"{timezone} is an illegal timezone";
            }

            return (convertedTimeZone, error);
        }

        private static (string, string) ReturnFormatTimeStampStr(DateTime datetime, string format)
        {
            string result = null;
            string error = null;
            try
            {
                result = datetime.ToString(format);
            }
            catch
            {
                error = $"illegal format representation: {format}";
            }

            return (result, error);
        }

        private static (string, string) ConvertFromUTC(string utcTimestamp, string timezone, string format)
        {
            string error = null;
            string result = null;
            var utcDt = DateTime.UtcNow;
            object parsed = null;
            object convertedTimeZone = null;
            (parsed, error) = ParseISOTimestamp(utcTimestamp);
            if (error == null)
            {
                utcDt = ((DateTime)parsed).ToUniversalTime();
            }

            if (error == null)
            {
                (convertedTimeZone, error) = ConvertTimeZoneFormat(timezone);

                if (error == null)
                {
                    var convertedDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDt, (TimeZoneInfo)convertedTimeZone);
                    (result, error) = ReturnFormatTimeStampStr(convertedDateTime, format);
                }
            }

            return (result, error);
        }

        private static (string, string) ConvertToUTC(string sourceTimestamp, string sourceTimezone, string format)
        {
            string error = null;
            string result = null;
            var srcDt = DateTime.UtcNow;
            try
            {
                srcDt = DateTime.Parse(sourceTimestamp);
            }
            catch
            {
                error = $"illegal time-stamp representation {sourceTimestamp}";
            }

            if (error == null)
            {
                object convertedTimeZone;
                (convertedTimeZone, error) = ConvertTimeZoneFormat(sourceTimezone);
                if (error == null)
                {
                    var convertedDateTime = TimeZoneInfo.ConvertTimeToUtc(srcDt, (TimeZoneInfo)convertedTimeZone);
                    (result, error) = ReturnFormatTimeStampStr(convertedDateTime, format);
                }
            }

            return (result, error);
        }

        private static (string, string) AddToTime(string timestamp, int interval, string timeUnit, string format)
        {
            string result = null;
            string error = null;
            object parsed = null;
            (parsed, error) = ParseISOTimestamp(timestamp);
            if (error == null)
            {
                var ts = (DateTime)parsed;
                Func<DateTime, DateTime> converter;
                (converter, error) = DateTimeConverter(interval, timeUnit, false);
                if (error == null)
                {
                    var addedTimeStamp = converter(ts);
                    (result, error) = ReturnFormatTimeStampStr(addedTimeStamp, format);
                }
            }

            return (result, error);
        }

        private static (object, string) StartOfDay(string timestamp, string format)
        {
            string result = null;
            string error = null;
            object parsed = null;
            (parsed, error) = ParseISOTimestamp(timestamp);

            if (error == null)
            {
                var ts = (DateTime)parsed;
                var startOfDay = ts.Date;
                (result, error) = ReturnFormatTimeStampStr(startOfDay, format);
            }

            return (result, error);
        }

        private static (object, string) StartOfHour(string timestamp, string format)
        {
            string result = null;
            string error = null;
            object parsed = null;
            (parsed, error) = ParseISOTimestamp(timestamp);

            if (error == null)
            {
                var ts = (DateTime)parsed;
                var startOfDay = ts.Date;
                var hours = ts.Hour;
                var startOfHour = startOfDay.AddHours(hours);
                (result, error) = ReturnFormatTimeStampStr(startOfHour, format);
            }

            return (result, error);
        }

        private static (object, string) StartOfMonth(string timestamp, string format)
        {
            string result = null;
            object parsed = null;
            string error = null;
            (parsed, error) = ParseISOTimestamp(timestamp);

            if (error == null)
            {
                var ts = (DateTime)parsed;
                var startOfDay = ts.Date;
                var days = ts.Day;
                var startOfMonth = startOfDay.AddDays(1 - days);
                (result, error) = ReturnFormatTimeStampStr(startOfMonth, format);
            }

            return (result, error);
        }

        private static (object, string) Ticks(string timestamp)
        {
            object result = null;
            object parsed = null;
            string error = null;
            (parsed, error) = ParseISOTimestamp(timestamp);

            if (error == null)
            {
                var ts = (DateTime)parsed;
                result = ts.Ticks;
            }

            return (result, error);
        }

        // URI Parsing Functions
        private static (object, string) ParseUri(string uri)
        {
            object result = null;
            string error = null;
            try
            {
                result = new Uri(uri);
            }
            catch
            {
                error = $"{uri} is an illegal URI string";
            }

            return (result, error);
        }

        private static (object, string) UriHost(string uri)
        {
            var (result, error) = ParseUri(uri);

            if (error == null)
            {
                try
                {
                    var uriBase = (Uri)result;
                    var host = uriBase.Host;
                    result = host.ToString();
                }
                catch
                {
                    error = "invalid operation, input uri should be an absolute URI";
                }
            }

            return (result, error);
        }

        private static (object, string) UriPath(string uri)
        {
            var (result, error) = ParseUri(uri);

            if (error == null)
            {
                try
                {
                    var uriBase = (Uri)result;
                    result = uriBase.AbsolutePath.ToString();
                }
                catch
                {
                    error = "invalid operation, input uri should be an absolute URI";
                }
            }

            return (result, error);
        }

        private static (object, string) UriPathAndQuery(string uri)
        {
            object result = null;
            string error = null;
            Uri uriBase = null;
            try
            {
                uriBase = new Uri(uri);
            }
            catch
            {
                error = "illegal URI string";
            }

            if (error == null)
            {
                try
                {
                    var pathAndQuery = uriBase.PathAndQuery;
                    result = pathAndQuery.ToString();
                }
                catch
                {
                    error = "invalid operation, input uri should be an absolute URI";
                }
            }

            return (result, error);
        }

        private static (object, string) UriPort(string uri)
        {
            var (result, error) = ParseUri(uri);
            if (error == null)
            {
                try
                {
                    var uriBase = (Uri)result;
                    var port = uriBase.Port;
                    result = Convert.ToInt32(port);
                }
                catch
                {
                    error = "invalid operation, input uri should be an absolute URI";
                }
            }

            return (result, error);
        }

        private static (object, string) UriQuery(string uri)
        {
            var (result, error) = ParseUri(uri);
            if (error == null)
            {
                try
                {
                    var uriBase = (Uri)result;
                    var query = uriBase.Query;
                    result = query.ToString();
                }
                catch
                {
                    error = "invalid operation, input uri should be an absolute URI";
                }
            }

            return (result, error);
        }

        private static (object, string) UriScheme(string uri)
        {
            var (result, error) = ParseUri(uri);

            if (error == null)
            {
                try
                {
                    var uriBase = (Uri)result;
                    var scheme = uriBase.Scheme;
                    result = scheme.ToString();
                }
                catch
                {
                    error = "invalid operation, input uri should be an absolute URI";
                }
            }

            return (result, error);
        }

        private static string AddOrdinal(int num)
        {
            var hasResult = false;
            var ordinalResult = num.ToString();
            if (num > 0)
            {
                switch (num % 100)
                {
                    case 11:
                    case 12:
                    case 13:
                        ordinalResult += "th";
                        hasResult = true;
                        break;
                }

                if (!hasResult)
                {
                    switch (num % 10)
                    {
                        case 1:
                            ordinalResult += "st";
                            break;
                        case 2:
                            ordinalResult += "nd";
                            break;
                        case 3:
                            ordinalResult += "rd";
                            break;
                        default:
                            ordinalResult += "th";
                            break;
                    }
                }
            }

            return ordinalResult;
        }

        // object manipulation
        private static object Coalesce(object[] objectList)
        {
            foreach (var obj in objectList)
            {
                if (obj != null)
                {
                    return obj;
                }
            }

            return null;
        }

        private static (object, string) XPath(object xmlObj, object xpath)
        {
            object value = null;
            object result = null;
            string error = null;
            var doc = new XmlDocument();
            try
            {
                doc.LoadXml(xmlObj.ToString());
            }
            catch
            {
                error = "not valid xml input";
            }

            if (error == null)
            {
                var nav = doc.CreateNavigator();
                var strExpr = xpath.ToString();
                var nodeList = new List<string>();
                try
                {
                    value = nav.Evaluate(strExpr);
                    if (value is IEnumerable)
                    {
                        var iterNodes = nav.Select(strExpr);
                        while (iterNodes.MoveNext())
                        {
                            var nodeType = (System.Xml.XmlNodeType)iterNodes.Current.NodeType;
                            var name = iterNodes.Current.Name;
                            var nameSpaceURI = iterNodes.Current.NamespaceURI.ToString();
                            var node = doc.CreateNode(nodeType, name, nameSpaceURI);
                            node.InnerText = iterNodes.Current.Value;
                            nodeList.Add(node.OuterXml.ToString());
                        }

                        if (nodeList.Count == 0)
                        {
                            error = "there is no matched nodes in the xml";
                        }
                    }
                }
                catch
                {
                    error = $"cannot evaluate the xpath query expression: {xpath.ToString()}";
                }

                if (error == null)
                {
                    if (nodeList.Count >= 1)
                    {
                        result = nodeList.ToArray();
                    }
                    else
                    {
                        result = value;
                    }
                }
            }

            return (result, error);
        }

        private static (object, string) JPath(object jsonEntity, string jpath)
        {
            object result = null;
            string error = null;
            object value = null;
            JObject jsonObj = null;
            if (jsonEntity is string jsonStr)
            {
                try
                {
                    jsonObj = JObject.Parse(jsonStr);
                }
                catch
                {
                    error = $"{jsonStr} is not a valid JSON string";
                }
            }
            else if (jsonEntity is JObject parsed)
            {
                jsonObj = parsed;
            }
            else
            {
                error = $"{jsonEntity} is not a valid JSON object or a valid JSON string";
            }

            if (error == null)
            {
                try
                {
                    value = jsonObj.SelectTokens(jpath);
                }
                catch
                {
                    error = $"{jpath} is not a valid path";
                }
            }

            if (error == null)
            {
                if (value is IEnumerable<JToken> products)
                {
                    if (products.Count() == 1)
                    {
                        result = ResolveValue(products.ElementAt(0));
                    }
                    else if (products.Count() > 1)
                    {
                        var nodeList = new List<object>();
                        foreach (var item in products)
                        {
                            nodeList.Add(ResolveValue(item));
                        }

                        result = nodeList;
                    }
                    else
                    {
                        error = $"there is no matching node for path: ${jpath} in the given JSON";
                    }
                }
            }

            return (result, error);
        }

        // conversion functions
        private static string ToBinary(string strToConvert)
        {
            var result = string.Empty;
            foreach (var element in strToConvert.ToCharArray())
            {
                result += Convert.ToString(element, 2).PadLeft(8, '0');
            }

            return result;
        }

        private static (object, string) ToXml(object contentToConvert)
        {
            string error = null;
            XDocument xml;
            string result = null;
            try
            {
                if (contentToConvert is string str)
                {
                    xml = XDocument.Load(JsonReaderWriterFactory.CreateJsonReader(Encoding.ASCII.GetBytes(str), new XmlDictionaryReaderQuotas()));
                }
                else
                {
                    xml = XDocument.Load(JsonReaderWriterFactory.CreateJsonReader(Encoding.ASCII.GetBytes(contentToConvert.ToString()), new XmlDictionaryReaderQuotas()));
                }

                result = xml.ToString().TrimStart('{').TrimEnd('}');
            }
            catch
            {
                error = "Invalid json";
            }

            return (result, error);
        }

        // collection functions
        private static (object value, string error) Skip(Expression expression, object state)
        {
            object result = null;
            string error;
            object arr;
            (arr, error) = expression.Children[0].TryEvaluate(state);

            if (error == null)
            {
                if (TryParseList(arr, out var list))
                {
                    int start = 0;
                    var startExpr = expression.Children[1];
                    (start, error) = startExpr.TryEvaluate<int>(state);
                    if (error == null && (start < 0 || start >= list.Count))
                    {
                        error = $"{startExpr}={start} which is out of range for {arr}";
                    }

                    if (error == null)
                    {
                        result = list.OfType<object>().Skip(start).ToList();
                    }
                }
                else
                {
                    error = $"{expression.Children[0]} is not array.";
                }
            }

            return (result, error);
        }

        private static (object, string) Take(Expression expression, object state)
        {
            object result = null;
            string error;
            object arr;
            (arr, error) = expression.Children[0].TryEvaluate(state);
            if (error == null)
            {
                var arrIsList = TryParseList(arr, out var list);
                var arrIsStr = arr.GetType() == typeof(string);
                if (arrIsList || arrIsStr)
                {
                    int count;
                    var countExpr = expression.Children[1];
                    (count, error) = countExpr.TryEvaluate<int>(state);
                    if (error == null)
                    {
                        if (arrIsList)
                        {
                            if (count < 0 || count >= list.Count)
                            {
                                error = $"{countExpr}={count} which is out of range for {arr}";
                            }
                            else
                            {
                                result = list.OfType<object>().Take(count).ToList();
                            }
                        }
                        else
                        {
                            if (count < 0 || count > list.Count)
                            {
                                error = $"{countExpr}={count} which is out of range for {arr}";
                            }
                            else
                            {
                                result = arr.ToString().Substring(0, count);
                            }
                        }
                    }
                }
                else
                {
                    error = $"{expression.Children[0]} is not array or string.";
                }
            }

            return (result, error);
        }

        private static (object, string) SubArray(Expression expression, object state)
        {
            object result = null;
            string error;
            object arr;
            (arr, error) = expression.Children[0].TryEvaluate(state);

            if (error == null)
            {
                if (TryParseList(arr, out var list))
                {
                    var startExpr = expression.Children[1];
                    int start;
                    (start, error) = startExpr.TryEvaluate<int>(state);
                    if (error == null)
                    {
                        if (error == null && (start < 0 || start > list.Count))
                        {
                            error = $"{startExpr}={start} which is out of range for {arr}";
                        }

                        if (error == null)
                        {
                            int end = 0;
                            if (expression.Children.Length == 2)
                            {
                                end = list.Count;
                            }
                            else
                            {
                                var endExpr = expression.Children[2];
                                (end, error) = endExpr.TryEvaluate<int>(state);
                                if (error == null && (end < 0 || end > list.Count))
                                {
                                    error = $"{endExpr}={end} which is out of range for {arr}";
                                }
                            }

                            if (error == null)
                            {
                                result = list.OfType<object>().Skip(start).Take(end - start).ToList();
                            }
                        }
                    }
                }
                else
                {
                    error = $"{expression.Children[0]} is not array.";
                }
            }

            return (result, error);
        }

        private static EvaluateExpressionDelegate SortBy(bool isDescending)
           => (expression, state) =>
           {
               object result = null;
               string error;
               object arr;
               (arr, error) = expression.Children[0].TryEvaluate(state);

               if (error == null)
               {
                   if (TryParseList(arr, out var list))
                   {
                       if (expression.Children.Length == 1)
                       {
                           if (isDescending)
                           {
                               result = list.OfType<object>().OrderByDescending(item => item).ToList();
                           }
                           else
                           {
                               result = list.OfType<object>().OrderBy(item => item).ToList();
                           }
                       }
                       else
                       {
                           var jarray = JArray.FromObject(list.OfType<object>().ToList());
                           var propertyNameExpression = expression.Children[1];
                           string propertyName;
                           (propertyName, error) = propertyNameExpression.TryEvaluate<string>(state);
                           if (error == null)
                           {
                               propertyName = propertyName ?? string.Empty;
                               if (isDescending)
                               {
                                   result = jarray.OrderByDescending(obj => obj[propertyName]).ToList();
                               }
                               else
                               {
                                   result = jarray.OrderBy(obj => obj[propertyName]).ToList();
                               }
                           }
                       }
                   }
                   else
                   {
                       error = $"{expression.Children[0]} is not array";
                   }
               }

               return (result, error);
           };

        // TODO we should uniform the list format
        private static List<object> Object2List(JObject jobj)
        {
            var tempList = new List<object>();
            foreach (var item in jobj)
            {
                tempList.Add(new { index = item.Key, value = item.Value });
            }

            return tempList;
        }

        private static (object, string) IndicesAndValues(Expression expression, object state)
        {
            object result = null;
            string error;
            object instance;
            (instance, error) = expression.Children[0].TryEvaluate(state);
            if (error == null)
            {
                if (TryParseList(instance, out var list))
                {
                    var tempList = new List<object>();
                    for (var i = 0; i < list.Count; i++)
                    {
                        tempList.Add(new { index = i, value = list[i] });
                    }

                    result = tempList;
                }
                else if (instance is JObject jobj)
                {
                    result = Object2List(jobj);
                }
                else if (JToken.FromObject(instance) is JObject jobject)
                {
                    result = Object2List(jobject);
                }
                else
                {
                    error = $"{expression.Children[0]} is not array or object..";
                }
            }

            return (result, error);
        }

        private static bool IsEqual(IReadOnlyList<object> args)
        {
            if (args[0] == null)
            {
                return args[1] == null;
            }

            if (TryParseList(args[0], out IList l0) && l0.Count == 0 && (TryParseList(args[1], out IList l1) && l1.Count == 0))
            {
                return true;
            }

            if (GetPropertyCount(args[0]) == 0 && GetPropertyCount(args[1]) == 0)
            {
                return true;
            }

            if (args[0].IsNumber() && args[0].IsNumber())
            {
                if (Math.Abs(Convert.ToDouble(args[0]) - Convert.ToDouble(args[1])) < 0.00000001)
                {
                    return true;
                }
            }

            try
            {
                return args[0] == args[1] || (args[0] != null && args[0].Equals(args[1]));
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get the property count of an object.
        /// </summary>
        /// <param name="obj">input object.</param>
        /// <returns>property count.</returns>
        private static int GetPropertyCount(object obj)
        {
            if (obj is IDictionary dictionary)
            {
                return dictionary.Count;
            }
            else if (obj is JObject jobj)
            {
                return jobj.Properties().Count();
            }
            else if (!(obj is JValue) && obj.GetType().IsValueType == false && obj.GetType().FullName != "System.String")
            {
                // exclude constant type.
                return obj.GetType().GetProperties().Length;
            }

            return -1;
        }

        private static IEnumerable<object> Flatten(IEnumerable<object> list, int dept)
        {
            var result = list.ToList();
            if (dept < 1)
            {
                dept = 1;
            }

            for (var i = 0; i < dept; i++)
            {
                var hasArray = result.Any(u => TryParseList(u, out var _));
                if (hasArray)
                {
                    var tempList = new List<object>();
                    foreach (var item in result)
                    {
                        if (TryParseList(item, out var itemList))
                        {
                            foreach (var childItem in itemList)
                            {
                                tempList.Add(childItem);
                            }
                        }
                        else
                        {
                            tempList.Add(item);
                        }
                    }

                    result = tempList.ToList();
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        private static IDictionary<string, ExpressionEvaluator> GetStandardFunctions()
        {
            var functions = new List<ExpressionEvaluator>
            {
                // Math
                new ExpressionEvaluator(ExpressionType.Element, ExtractElement, ReturnType.Object, ValidateBinary),
                MultivariateNumeric(ExpressionType.Subtract, args => Subtract(args[0], args[1])),
                MultivariateNumeric(ExpressionType.Multiply, args => Multiply(args[0], args[1])),
                MultivariateNumeric(
                    ExpressionType.Divide,
                    args => Divide(args[0], args[1]),
                    (val, expression, pos) =>
                    {
                        var error = VerifyNumber(val, expression, pos);
                        if (error == null && (pos > 0 && Convert.ToSingle(val) == 0.0))
                        {
                            error = $"Cannot divide by 0 from {expression}";
                        }

                        return error;
                    }),
                NumericOrCollection(ExpressionType.Min, (args) =>
                {
                    object result = double.MaxValue;
                    if (args.Count == 1)
                    {
                        if (TryParseList(args[0], out IList ilist))
                        {
                            foreach (var value in ilist)
                            {
                                result = Min(result, value);
                            }
                        }
                        else
                        {
                            result = Min(result, args[0]);
                        }
                    }
                    else
                    {
                        foreach (var arg in args)
                        {
                            if (TryParseList(arg, out IList ilist))
                            {
                                foreach (var value in ilist)
                                {
                                    result = Min(result, value);
                                }
                            }
                            else
                            {
                                result = Min(result, arg);
                            }
                        }
                    }

                    return result;
                }),
                NumericOrCollection(ExpressionType.Max, args =>
                {
                    object result = double.MinValue;
                    if (args.Count == 1)
                    {
                        if (TryParseList(args[0], out IList ilist))
                        {
                            foreach (var value in ilist)
                            {
                                result = Max(result, value);
                            }
                        }
                        else
                        {
                            result = Max(result, args[0]);
                        }
                    }
                    else
                    {
                        foreach (var arg in args)
                        {
                            if (TryParseList(arg, out IList ilist))
                            {
                                foreach (var value in ilist)
                                {
                                    result = Max(result, value);
                                }
                            }
                            else
                            {
                                result = Max(result, arg);
                            }
                        }
                    }

                    return result;
                }),
                MultivariateNumeric(ExpressionType.Power, args => Math.Pow(Convert.ToDouble(args[0]), Convert.ToDouble(args[1]))),
                new ExpressionEvaluator(
                    ExpressionType.Mod,
                    ApplyWithError(
                        args =>
                        {
                            object value = null;
                            string error;
                            if (Convert.ToInt32(args[1]) == 0)
                            {
                                error = $"Cannot mod by 0";
                            }
                            else
                            {
                                error = null;
                                value = Mod(args[0], args[1]);
                            }

                            return (value, error);
                        },
                        VerifyInteger),
                    ReturnType.Number,
                    ValidateBinaryNumber),
                new ExpressionEvaluator(
                    ExpressionType.Average,
                    Apply(
                        args =>
                        {
                            var operands = ResolveListValue(args[0]).OfType<object>().ToList();
                            return operands.Average(u => Convert.ToSingle(u));
                        },
                        VerifyNumericList),
                    ReturnType.Number,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.Add,
                    ApplySequenceWithError(
                        args =>
                        {
                            object result = null;
                            string error = null;
                            var firstItem = args[0];
                            var secondItem = args[1];
                            var stringConcat = !firstItem.IsNumber() || !secondItem.IsNumber();

                            if ((firstItem == null && secondItem.IsNumber())
                                || (secondItem == null && firstItem.IsNumber()))
                            {
                                error = "Operator '+' or add cannot be applied to operands of type 'number' and null object.";
                            }
                            else
                            {
                                if (stringConcat)
                                {
                                    result = $"{firstItem?.ToString()}{secondItem?.ToString()}";
                                }
                                else
                                {
                                    result = Add(args[0], args[1]);
                                }
                            }

                            return (result, error);
                        }, VerifyNumberOrStringOrNull),
                    ReturnType.Object,
                    (expression) => ValidateArityAndAnyType(expression, 2, int.MaxValue)),
                new ExpressionEvaluator(
                    ExpressionType.Sum,
                    Apply(
                        args =>
                        {
                            var operands = ResolveListValue(args[0]).OfType<object>().ToList();
                            return operands.All(u => u.IsInteger()) ? operands.Sum(u => Convert.ToInt32(u)) : operands.Sum(u => Convert.ToSingle(u));
                        },
                        VerifyNumericList),
                    ReturnType.Number,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.Range,
                    ApplyWithError(
                        args =>
                        {
                            string error = null;
                            IList result = null;
                            var count = Convert.ToInt32(args[1]);
                            if (count <= 0)
                            {
                                error = $"The second parameter should be more than zero";
                            }
                            else
                            {
                                result = Enumerable.Range(Convert.ToInt32(args[0]), count).ToList();
                            }

                            return (result, error);
                        },
                        VerifyInteger),
                    ReturnType.Object,
                    ValidateBinaryNumber),

                // Collection Functions
                new ExpressionEvaluator(
                    ExpressionType.Count,
                    Apply(
                        args =>
                        {
                            object count = null;
                            if (args[0] is string string0)
                            {
                                count = string0.Length;
                            }
                            else if (args[0] is IList list)
                            {
                                count = list.Count;
                            }

                            return count;
                        }, VerifyContainer),
                    ReturnType.Number,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.Union,
                    Apply(
                        args =>
                        {
                        var result = (IEnumerable<object>)args[0];
                        for (var i = 1; i < args.Count; i++)
                        {
                            var nextItem = (IEnumerable<object>)args[i];
                            result = result.Union(nextItem);
                        }

                        return result.ToList();
                        }, VerifyList),
                    ReturnType.Object,
                    ValidateAtLeastOne),
                new ExpressionEvaluator(
                    ExpressionType.Intersection,
                    Apply(
                        args =>
                        {
                        var result = (IEnumerable<object>)args[0];
                        for (var i = 1; i < args.Count; i++)
                        {
                            var nextItem = (IEnumerable<object>)args[i];
                            result = result.Intersect(nextItem);
                        }

                        return result.ToList();
                        }, VerifyList),
                    ReturnType.Object,
                    ValidateAtLeastOne),
                new ExpressionEvaluator(
                    ExpressionType.Skip,
                    Skip,
                    ReturnType.Object,
                    (expression) => ValidateOrder(expression, null, ReturnType.Object, ReturnType.Number)),
                new ExpressionEvaluator(
                    ExpressionType.Take,
                    Take,
                    ReturnType.Object,
                    (expression) => ValidateOrder(expression, null, ReturnType.Object, ReturnType.Number)),
                new ExpressionEvaluator(
                    ExpressionType.SubArray,
                    SubArray,
                    ReturnType.Object,
                    (expression) => ValidateOrder(expression, new[] { ReturnType.Number }, ReturnType.Object, ReturnType.Number)),
                new ExpressionEvaluator(
                    ExpressionType.SortBy,
                    SortBy(false),
                    ReturnType.Object,
                    (expression) => ValidateOrder(expression, new[] { ReturnType.String }, ReturnType.Object)),
                new ExpressionEvaluator(
                    ExpressionType.SortByDescending,
                    SortBy(true),
                    ReturnType.Object,
                    (expression) => ValidateOrder(expression, new[] { ReturnType.String }, ReturnType.Object)),
                new ExpressionEvaluator(ExpressionType.IndicesAndValues, IndicesAndValues, ReturnType.Object, ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.Flatten,
                    Apply(
                        args =>
                        {
                            var depth = args.Count > 1 ? Convert.ToInt32(args[1]) : 100;
                            return Flatten((IEnumerable<object>)args[0], depth);
                        }),
                    ReturnType.Object,
                    (expression) => ValidateOrder(expression, new[] { ReturnType.Number }, ReturnType.Object)),
                new ExpressionEvaluator(
                    ExpressionType.Unique,
                    Apply(
                        args =>
                        {
                            return ((IEnumerable<object>)args[0]).Distinct().ToList();
                        }, VerifyList),
                    ReturnType.Object,
                    (expression) => ValidateOrder(expression, null, ReturnType.Object)),

                // Booleans
                Comparison(ExpressionType.LessThan, args => Convert.ToDouble(args[0]) < Convert.ToDouble(args[1]), ValidateBinaryNumberOrString, VerifyNumberOrString),
                Comparison(ExpressionType.LessThanOrEqual, args => Convert.ToDouble(args[0]) <= Convert.ToDouble(args[1]), ValidateBinaryNumberOrString, VerifyNumberOrString),

                Comparison(ExpressionType.Equal, IsEqual, ValidateBinary),
                Comparison(ExpressionType.NotEqual, args => !IsEqual(args), ValidateBinary),
                Comparison(ExpressionType.GreaterThan, args => Convert.ToDouble(args[0]) > Convert.ToDouble(args[1]), ValidateBinaryNumberOrString, VerifyNumberOrString),
                Comparison(ExpressionType.GreaterThanOrEqual, args => Convert.ToDouble(args[0]) >= Convert.ToDouble(args[1]), ValidateBinaryNumberOrString, VerifyNumberOrString),
                Comparison(ExpressionType.Exists, args => args[0] != null, ValidateUnary, VerifyNotNull),
                new ExpressionEvaluator(
                    ExpressionType.Contains,
                    (expression, state) =>
                    {
                        var found = false;
                        var (args, error) = EvaluateChildren(expression, state);
                        if (error == null)
                        {
                            if (args[0] is string string0 && args[1] is string string1)
                            {
                                found = string0.Contains(string1);
                            }
                            else if (TryParseList(args[0], out IList ilist))
                            {
                                // list to find a value
                                var operands = ResolveListValue(ilist);
                                found = operands.Contains(args[1]);
                            }
                            else if (args[1] is string string2)
                            {
                                found = TryAccessProperty((object)args[0], string2, out var _);
                            }
                        }

                        return (found, null);
                    },
                    ReturnType.Boolean,
                    ValidateBinary),
                Comparison(ExpressionType.Empty, args => IsEmpty(args[0]), ValidateUnary, VerifyContainer),
                new ExpressionEvaluator(ExpressionType.And, (expression, state) => And(expression, state), ReturnType.Boolean, ValidateAtLeastOne),
                new ExpressionEvaluator(ExpressionType.Or, (expression, state) => Or(expression, state), ReturnType.Boolean, ValidateAtLeastOne),
                new ExpressionEvaluator(ExpressionType.Not, (expression, state) => Not(expression, state), ReturnType.Boolean, ValidateUnary),

                // String
                new ExpressionEvaluator(
                    ExpressionType.Concat,
                    Apply(
                        args =>
                        {
                            var builder = new StringBuilder();
                            foreach (var arg in args)
                            {
                                if (arg != null)
                                {
                                    builder.Append(arg.ToString());
                                }
                            }

                            return builder.ToString();
                        }),
                    ReturnType.String,
                    ValidateAtLeastOne),
                new ExpressionEvaluator(
                    ExpressionType.Length,
                    Apply(
                        args =>
                            {
                                var result = 0;
                                if (args[0] is string str)
                                    {
                                        result = str.Length;
                                    }
                                else
                                    {
                                        result = 0;
                                    }

                                return result;
                            }, VerifyStringOrNull),
                    ReturnType.Number,
                    ValidateAtLeastOne),
                new ExpressionEvaluator(
                    ExpressionType.Replace,
                    ApplyWithError(
                        args =>
                        {
                            string error = null;
                            string result = null;
                            string inputStr = ParseStringOrNull(args[0]);
                            string oldStr = ParseStringOrNull(args[1]);
                            if (oldStr.Length == 0)
                            {
                                error = $"{args[1]} the oldValue in replace function should be a string with at least length 1.";
                            }

                            string newStr = ParseStringOrNull(args[2]);
                            if (error == null)
                            {
                                result = inputStr.Replace(oldStr, newStr);
                            }

                            return (result, error);
                        }, VerifyStringOrNull),
                    ReturnType.String,
                    (expression) => ValidateArityAndAnyType(expression, 3, 3, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.ReplaceIgnoreCase,
                    ApplyWithError(
                        args =>
                        {
                            string error = null;
                            string result = null;
                            string inputStr = ParseStringOrNull(args[0]);
                            string oldStr = ParseStringOrNull(args[1]);
                            if (oldStr.Length == 0)
                            {
                                error = $"{args[1]} the oldValue in replace function should be a string with at least length 1.";
                            }

                            string newStr = ParseStringOrNull(args[2]);
                            if (error == null)
                            {
                                result = Regex.Replace(inputStr, oldStr, newStr, RegexOptions.IgnoreCase);
                            }

                            return (result, error);
                        }, VerifyStringOrNull),
                    ReturnType.String,
                    (expression) => ValidateArityAndAnyType(expression, 3, 3, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.Split,
                    Apply(
                        args =>
                        {
                            var inputStr = string.Empty;
                            var seperator = string.Empty;
                            if (args.Count == 1)
                            {
                                inputStr = ParseStringOrNull(args[0]);
                            }
                            else
                            {
                                inputStr = ParseStringOrNull(args[0]);
                                seperator = ParseStringOrNull(args[1]);
                            }

                            if (seperator == string.Empty)
                            {
                                return inputStr.Select(c => c.ToString()).ToArray();
                            }

                            return inputStr.Split(seperator.ToCharArray());
                        }, VerifyStringOrNull),
                    ReturnType.Object,
                    (expression) => ValidateArityAndAnyType(expression, 1, 2, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.Substring,
                    Substring,
                    ReturnType.String,
                    (expression) => ValidateOrder(expression, new[] { ReturnType.Number }, ReturnType.String, ReturnType.Number)),
                StringTransform(
                                ExpressionType.ToLower,
                                args =>
                                {
                                    if (args[0] == null)
                                    {
                                        return string.Empty;
                                    }
                                    else
                                    {
                                        return args[0].ToString().ToLower();
                                    }
                                }),
                StringTransform(
                                ExpressionType.ToUpper,
                                args =>
                                {
                                    if (args[0] == null)
                                    {
                                        return string.Empty;
                                    }
                                    else
                                    {
                                        return args[0].ToString().ToUpper();
                                    }
                                }),
                StringTransform(
                                ExpressionType.Trim,
                                args =>
                                {
                                    if (args[0] == null)
                                    {
                                        return string.Empty;
                                    }
                                    else
                                    {
                                        return args[0].ToString().Trim();
                                    }
                                }),
                new ExpressionEvaluator(
                    ExpressionType.StartsWith,
                    Apply(
                        args =>
                        {
                            string rawStr = ParseStringOrNull(args[0]);
                            string seekStr = ParseStringOrNull(args[1]);
                            return rawStr.StartsWith(seekStr);
                        }, VerifyStringOrNull),
                    ReturnType.Boolean,
                    (expression) => ValidateArityAndAnyType(expression, 2, 2, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.EndsWith,
                    Apply(
                        args =>
                        {
                            string rawStr = ParseStringOrNull(args[0]);
                            string seekStr = ParseStringOrNull(args[1]);
                            return rawStr.EndsWith(seekStr);
                        }, VerifyStringOrNull),
                    ReturnType.Boolean,
                    (expression) => ValidateArityAndAnyType(expression, 2, 2, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.CountWord,
                    Apply(
                        args =>
                        {
                            if (args[0] is string)
                            {
                                return Regex.Split(args[0].ToString().Trim(), @"\s{1,}").Length;
                            }
                            else
                            {
                                return 0;
                            }
                        }, VerifyStringOrNull),
                    ReturnType.Number,
                    ValidateUnaryString),
                new ExpressionEvaluator(
                    ExpressionType.AddOrdinal,
                    Apply(args => AddOrdinal(Convert.ToInt32(args[0])), VerifyInteger),
                    ReturnType.Number,
                    (expression) => ValidateArityAndAnyType(expression, 1, 1, ReturnType.Number)),
                new ExpressionEvaluator(
                    ExpressionType.Join,
                    (expression, state) =>
                    {
                        object result = null;
                        var (args, error) = EvaluateChildren(expression, state);
                        if (error == null)
                        {
                            if (!TryParseList(args[0], out IList list))
                            {
                                error = $"{expression.Children[0]} evaluates to {args[0]} which is not a list.";
                            }
                            else
                            {
                                if (args.Count == 2)
                                {
                                    result = string.Join(args[1].ToString(), list.OfType<object>().Select(x => x.ToString()));
                                }
                                else
                                {
                                    if (list.Count < 3)
                                    {
                                        result = string.Join(args[2].ToString(), list.OfType<object>().Select(x => x.ToString()));
                                    }
                                    else
                                    {
                                        var firstPart = string.Join(args[1].ToString(), list.OfType<object>().TakeWhile(o => o != null && o != list.OfType<object>().LastOrDefault()));
                                        result = firstPart + args[2] + list.OfType<object>().Last().ToString();
                                    }
                                }
                            }
                        }

                        return (result, error);
                    },
                    ReturnType.String,
                    expr => ValidateOrder(expr, new[] { ReturnType.String }, ReturnType.Object, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.NewGuid,
                    Apply(args => Guid.NewGuid().ToString()),
                    ReturnType.String,
                    (exprssion) => ValidateArityAndAnyType(exprssion, 0, 0)),
                new ExpressionEvaluator(
                    ExpressionType.IndexOf,
                    (expression, state) =>
                    {
                        object result = -1;
                        var (args, error) = EvaluateChildren(expression, state);
                        if (error == null)
                        {
                            if (args[0] is string || args[0] == null)
                            {
                                if (args[1] is string || args[1] == null)
                                {
                                    result = ParseStringOrNull(args[0]).IndexOf(ParseStringOrNull(args[1]));
                                }
                                else
                                {
                                    error = $"Can only look for indexof string in {expression}";
                                }
                            }
                            else if (TryParseList(args[0], out IList list))
                            {
                                result = ResolveListValue(list).IndexOf(args[1]);
                            }
                            else
                            {
                                error = $"{expression} works only on string or list.";
                            }
                        }

                        return (result, error);
                    },
                    ReturnType.Number,
                    (expression) => ValidateArityAndAnyType(expression, 2, 2, ReturnType.String, ReturnType.Boolean, ReturnType.Number, ReturnType.Object)),
                new ExpressionEvaluator(
                    ExpressionType.LastIndexOf,
                    (expression, state) =>
                    {
                        object result = -1;
                        var (args, error) = EvaluateChildren(expression, state);
                        if (error == null)
                        {
                            if (args[0] is string || args[0] == null)
                            {
                                if (args[1] is string || args[1] == null)
                                {
                                    result = ParseStringOrNull(args[0]).LastIndexOf(ParseStringOrNull(args[1]));
                                }
                                else
                                {
                                    error = $"Can only look for indexof string in {expression}";
                                }
                            }
                            else if (TryParseList(args[0], out IList list))
                            {
                                result = ResolveListValue(list).OfType<object>().ToList().LastIndexOf(args[1]);
                            }
                            else
                            {
                                error = $"{expression} works only on string or list.";
                            }
                        }

                        return (result, error);
                    },
                    ReturnType.Number,
                    (expression) => ValidateArityAndAnyType(expression, 2, 2, ReturnType.String, ReturnType.Boolean, ReturnType.Number, ReturnType.Object)),

                // Date and time
                TimeTransform(ExpressionType.AddDays, (ts, add) => ts.AddDays(add)),
                TimeTransform(ExpressionType.AddHours, (ts, add) => ts.AddHours(add)),
                TimeTransform(ExpressionType.AddMinutes, (ts, add) => ts.AddMinutes(add)),
                TimeTransform(ExpressionType.AddSeconds, (ts, add) => ts.AddSeconds(add)),
                new ExpressionEvaluator(
                    ExpressionType.DayOfMonth,
                    ApplyWithError(args => ParseISOTimestamp((string)args[0], dt => dt.Day), VerifyString),
                    ReturnType.Number,
                    ValidateUnaryString),
                new ExpressionEvaluator(
                    ExpressionType.DayOfWeek,
                    ApplyWithError(args => ParseISOTimestamp((string)args[0], dt => Convert.ToInt32(dt.DayOfWeek)), VerifyString),
                    ReturnType.Number,
                    ValidateUnaryString),
                new ExpressionEvaluator(
                    ExpressionType.DayOfYear,
                    ApplyWithError(args => ParseISOTimestamp((string)args[0], dt => dt.DayOfYear), VerifyString),
                    ReturnType.Number,
                    ValidateUnaryString),
                new ExpressionEvaluator(
                    ExpressionType.Month,
                    ApplyWithError(args => ParseISOTimestamp((string)args[0], dt => dt.Month), VerifyString),
                    ReturnType.Number,
                    ValidateUnaryString),
                new ExpressionEvaluator(
                    ExpressionType.Date,
                    ApplyWithError(args => ParseISOTimestamp((string)args[0], dt => dt.Date.ToString("M/dd/yyyy")), VerifyString),
                    ReturnType.String,
                    ValidateUnaryString),
                new ExpressionEvaluator(
                    ExpressionType.Year,
                    ApplyWithError(args => ParseISOTimestamp((string)args[0], dt => dt.Year), VerifyString),
                    ReturnType.Number,
                    ValidateUnaryString),
                new ExpressionEvaluator(
                    ExpressionType.UtcNow,
                    Apply(args => DateTime.UtcNow.ToString(args.Count() == 1 ? args[0].ToString() : DefaultDateTimeFormat), VerifyString),
                    ReturnType.String),
                new ExpressionEvaluator(
                    ExpressionType.FormatDateTime,
                    ApplyWithError(
                        args =>
                        {
                            object result = null;
                            string error = null;
                            object timestamp = args[0];
                            if (Extensions.IsNumber(timestamp))
                            {
                                if (double.TryParse(args[0].ToString(), out double unixTimestamp))
                                {
                                    var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                                    timestamp = dateTime.AddSeconds(unixTimestamp);
                                }
                            }

                            (result, error) = ParseTimestamp((string)timestamp.ToString(), dt => dt.ToString(args.Count() == 2 ? args[1].ToString() : DefaultDateTimeFormat));

                            return (result, error);
                        }),
                    ReturnType.String,
                    (expr) => ValidateOrder(expr, new[] { ReturnType.String }, ReturnType.Object)),
                new ExpressionEvaluator(
                    ExpressionType.SubtractFromTime,
                    (expr, state) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state);
                        if (error == null)
                        {
                            if (args[0] is string string0 && args[1].IsInteger() && args[2] is string string2)
                            {
                                var format = (args.Count() == 4) ? (string)args[3] : DefaultDateTimeFormat;
                                Func<DateTime, DateTime> timeConverter;
                                (timeConverter, error) = DateTimeConverter(Convert.ToInt32(args[1]), string2);
                                if (error == null)
                                {
                                    (value, error) = ParseISOTimestamp(string0, dt => timeConverter(dt).ToString(format));
                                }
                            }
                            else
                            {
                                error = $"{expr} can't evaluate.";
                            }
                        }

                        return (value, error);
                    },
                    ReturnType.String,
                    (expr) => ValidateOrder(expr, new[] { ReturnType.String }, ReturnType.String, ReturnType.Number, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.DateReadBack,
                    ApplyWithError(
                        args =>
                        {
                            object result = null;
                            string error;
                            (result, error) = ParseISOTimestamp((string)args[0]);
                            if (error == null)
                            {
                                var timestamp1 = (DateTime)result;
                                (result, error) = ParseISOTimestamp((string)args[1]);
                                if (error == null)
                                {
                                    var timestamp2 = (DateTime)result;
                                    var timex = new TimexProperty(timestamp2.ToString("yyyy-MM-dd"));
                                    result = TimexRelativeConvert.ConvertTimexToStringRelative(timex, timestamp1);
                                }
                            }

                            return (result, error);
                        },
                        VerifyString),
                    ReturnType.String,
                    expr => ValidateOrder(expr, null, ReturnType.String, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.GetTimeOfDay,
                    ApplyWithError(
                        args =>
                        {
                            object value = null;
                            string error = null;
                            (value, error) = ParseISOTimestamp((string)args[0]);
                            if (error == null)
                            {
                                var timestamp = (DateTime)value;
                                if (timestamp.Hour == 0 && timestamp.Minute == 0)
                                {
                                    value = "midnight";
                                }
                                else if (timestamp.Hour >= 0 && timestamp.Hour < 12)
                                {
                                    value = "morning";
                                }
                                else if (timestamp.Hour == 12 && timestamp.Minute == 0)
                                {
                                    value = "noon";
                                }
                                else if (timestamp.Hour < 18)
                                {
                                    value = "afternoon";
                                }
                                else if (timestamp.Hour < 22 || (timestamp.Hour == 22 && timestamp.Minute == 0))
                                {
                                    value = "evening";
                                }
                                else
                                {
                                    value = "night";
                                }
                            }

                            return (value, error);
                        },
                        VerifyString),
                    ReturnType.String,
                    expr => ValidateOrder(expr, null, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.GetFutureTime,
                    (expr, state) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state);
                        if (error == null)
                        {
                            if (args[0].IsInteger() && args[1] is string string1)
                            {
                                var format = (args.Count() == 3) ? (string)args[2] : DefaultDateTimeFormat;
                                Func<DateTime, DateTime> timeConverter;
                                (timeConverter, error) = DateTimeConverter(Convert.ToInt32(args[0]), string1, false);
                                if (error == null)
                                {
                                    value = timeConverter(DateTime.Now).ToString(format);
                                }
                            }
                            else
                            {
                                error = $"{expr} can't evaluate.";
                            }
                        }

                        return (value, error);
                    },
                    ReturnType.String,
                    (expr) => ValidateOrder(expr, new[] { ReturnType.String }, ReturnType.Number, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.GetPastTime,
                    (expr, state) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state);
                        if (error == null)
                        {
                            if (args[0].IsInteger() && args[1] is string string1)
                            {
                                var format = (args.Count() == 3) ? (string)args[2] : DefaultDateTimeFormat;
                                Func<DateTime, DateTime> timeConverter;
                                (timeConverter, error) = DateTimeConverter(Convert.ToInt32(args[0]), string1);
                                if (error == null)
                                {
                                    value = timeConverter(DateTime.Now).ToString(format);
                                }
                            }
                            else
                            {
                                error = $"{expr} can't evaluate.";
                            }
                        }

                        return (value, error);
                    },
                    ReturnType.String,
                    (expr) => ValidateOrder(expr, new[] { ReturnType.String }, ReturnType.Number, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.ConvertFromUtc,
                    (expr, state) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state);
                        if (error == null)
                        {
                            var format = (args.Count() == 3) ? (string)args[2] : DefaultDateTimeFormat;
                            if (args[0] is string timestamp && args[1] is string targetTimeZone)
                            {
                                (value, error) = ConvertFromUTC(timestamp, targetTimeZone, format);
                            }
                            else
                            {
                                error = $"{expr} can't evaluate.";
                            }
                        }

                        return (value, error);
                    },
                    ReturnType.String,
                    expr => ValidateArityAndAnyType(expr, 2, 3, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.ConvertToUtc,
                    (expr, state) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state);
                        if (error == null)
                        {
                            var format = (args.Count() == 3) ? (string)args[2] : DefaultDateTimeFormat;
                            if (args[0] is string timestamp && args[1] is string sourceTimeZone)
                            {
                                (value, error) = ConvertToUTC(timestamp, sourceTimeZone, format);
                            }
                            else
                            {
                                error = $"{expr} can't evaluate.";
                            }
                        }

                        return (value, error);
                    },
                    ReturnType.String,
                    expr => ValidateArityAndAnyType(expr, 2, 3, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.AddToTime,
                    (expr, state) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state);
                        if (error == null)
                        {
                            var format = (args.Count() == 4) ? (string)args[3] : DefaultDateTimeFormat;
                            if (args[0] is string timestamp && args[1].IsInteger() && args[2] is string timeUnit)
                            {
                                (value, error) = AddToTime(timestamp, Convert.ToInt32(args[1]), timeUnit, format);
                            }
                            else
                            {
                                error = $"{expr} can't evaluate.";
                            }
                        }

                        return (value, error);
                    },
                    ReturnType.String,
                    expr => ValidateOrder(expr, new[] { ReturnType.String }, ReturnType.String, ReturnType.Number, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.StartOfDay,
                    (expr, state) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state);
                        if (error == null)
                        {
                            var format = (args.Count() == 2) ? (string)args[1] : DefaultDateTimeFormat;
                            if (args[0] is string timestamp)
                            {
                                (value, error) = StartOfDay(timestamp, format);
                            }
                            else
                            {
                                error = $"{expr} can't evaluate.";
                            }
                        }

                        return (value, error);
                    },
                    ReturnType.String,
                    expr => ValidateArityAndAnyType(expr, 1, 2, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.StartOfHour,
                    (expr, state) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state);
                        if (error == null)
                        {
                            var format = (args.Count() == 2) ? (string)args[1] : DefaultDateTimeFormat;
                            if (args[0] is string timestamp)
                            {
                                (value, error) = StartOfHour(timestamp, format);
                            }
                            else
                            {
                                error = $"{expr} can't evaluate.";
                            }
                        }

                        return (value, error);
                    },
                    ReturnType.String,
                    expr => ValidateArityAndAnyType(expr, 1, 2, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.StartOfMonth,
                    (expr, state) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state);
                        if (error == null)
                        {
                            var format = (args.Count() == 2) ? (string)args[1] : DefaultDateTimeFormat;
                            if (args[0] is string timestamp)
                            {
                                (value, error) = StartOfMonth(timestamp, format);
                            }
                            else
                            {
                                error = $"{expr} can't evaluate.";
                            }
                        }

                        return (value, error);
                    },
                    ReturnType.String,
                    expr => ValidateArityAndAnyType(expr, 1, 2, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.Ticks,
                    (expr, state) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state);
                        if (error == null)
                        {
                            if (args[0] is string ts)
                            {
                                (value, error) = Ticks(ts);
                            }
                            else
                            {
                                error = $"{expr} can't evaluate.";
                            }
                        }

                        return (value, error);
                    },
                    ReturnType.Number,
                    expr => ValidateArityAndAnyType(expr, 1, 1, ReturnType.String)),

                // URI Parsing
                new ExpressionEvaluator(
                    ExpressionType.UriHost,
                    (expr, state) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state);
                        if (error == null)
                        {
                            if (args[0] is string uri)
                            {
                                (value, error) = UriHost(uri);
                            }
                            else
                            {
                                error = $"{expr} can't evaluate.";
                            }
                        }

                        return (value, error);
                    },
                    ReturnType.String,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.UriPath,
                    (expr, state) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state);
                        if (error == null)
                        {
                            if (args[0] is string uri)
                            {
                                (value, error) = UriPath(uri);
                            }
                            else
                            {
                                error = $"{expr} can't evaluate.";
                            }
                        }

                        return (value, error);
                    },
                    ReturnType.String,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.UriPathAndQuery,
                    (expr, state) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state);
                        if (error == null)
                        {
                            if (args[0] is string uri)
                            {
                                (value, error) = UriPathAndQuery(uri);
                            }
                            else
                            {
                                error = $"{expr} can't evaluate.";
                            }
                        }

                        return (value, error);
                    },
                    ReturnType.String,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.UriPort,
                    (expr, state) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state);
                        if (error == null)
                        {
                            if (args[0] is string uri)
                            {
                                (value, error) = UriPort(uri);
                            }
                            else
                            {
                                error = $"{expr} can't evaluate.";
                            }
                        }

                        return (value, error);
                    },
                    ReturnType.Number,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.UriQuery,
                    (expr, state) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state);
                        if (error == null)
                        {
                            if (args[0] is string uri)
                            {
                                (value, error) = UriQuery(uri);
                            }
                            else
                            {
                                error = $"{expr} can't evaluate.";
                            }
                        }

                        return (value, error);
                    },
                    ReturnType.String,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.UriScheme,
                    (expr, state) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state);
                        if (error == null)
                        {
                            if (args[0] is string uri)
                            {
                                (value, error) = UriScheme(uri);
                            }
                            else
                            {
                                error = $"{expr} can't evaluate.";
                            }
                        }

                        return (value, error);
                    },
                    ReturnType.String,
                    ValidateUnary),

                // Conversions
                new ExpressionEvaluator(ExpressionType.Float, Apply(args => Convert.ToDouble(args[0])), ReturnType.Number, ValidateUnary),
                new ExpressionEvaluator(ExpressionType.Int, Apply(args => Convert.ToInt32(args[0])), ReturnType.Number, ValidateUnary),
                new ExpressionEvaluator(ExpressionType.Binary, Apply(args => ToBinary(args[0].ToString()), VerifyString), ReturnType.String, ValidateUnary),
                new ExpressionEvaluator(ExpressionType.Base64, Apply(args => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(args[0].ToString())), VerifyString), ReturnType.String, ValidateUnary),
                new ExpressionEvaluator(ExpressionType.Base64ToBinary, Apply(args => ToBinary(args[0].ToString()), VerifyString), ReturnType.String, ValidateUnary),
                new ExpressionEvaluator(ExpressionType.Base64ToString, Apply(args => System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(args[0].ToString())), VerifyString), ReturnType.String, ValidateUnary),
                new ExpressionEvaluator(ExpressionType.UriComponent, Apply(args => Uri.EscapeDataString(args[0].ToString()), VerifyString), ReturnType.String, ValidateUnary),
                new ExpressionEvaluator(ExpressionType.DataUri, Apply(args => "data:text/plain;charset=utf-8;base64," + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(args[0].ToString())), VerifyString), ReturnType.String, ValidateUnary),
                new ExpressionEvaluator(ExpressionType.DataUriToBinary, Apply(args => ToBinary(args[0].ToString()), VerifyString), ReturnType.String, ValidateUnary),
                new ExpressionEvaluator(ExpressionType.DataUriToString, Apply(args => System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(args[0].ToString().Substring(args[0].ToString().IndexOf(",") + 1))), VerifyString), ReturnType.String, ValidateUnary),
                new ExpressionEvaluator(ExpressionType.UriComponentToString, Apply(args => Uri.UnescapeDataString(args[0].ToString()), VerifyString), ReturnType.String, ValidateUnary),

                // TODO: Is this really the best way?
                new ExpressionEvaluator(ExpressionType.String, Apply(args => JsonConvert.SerializeObject(args[0]).TrimStart('"').TrimEnd('"')), ReturnType.String, ValidateUnary),
                Comparison(ExpressionType.Bool, args => IsLogicTrue(args[0]), ValidateUnary),
                new ExpressionEvaluator(ExpressionType.Xml, ApplyWithError(args => ToXml(args[0])), ReturnType.String, ValidateUnary),

                // Misc
                new ExpressionEvaluator(ExpressionType.Accessor, Accessor, ReturnType.Object, ValidateAccessor),
                new ExpressionEvaluator(ExpressionType.GetProperty, GetProperty, ReturnType.Object, (expr) => ValidateOrder(expr, null, ReturnType.Object, ReturnType.String)),
                new ExpressionEvaluator(ExpressionType.If, (expression, state) => If(expression, state), ReturnType.Object, (expression) => ValidateArityAndAnyType(expression, 3, 3)),
                new ExpressionEvaluator(
                    ExpressionType.Rand,
                    ApplyWithError(
                        args =>
                        {
                            object value = null;
                            string error = null;
                            var min = Convert.ToInt32(args[0]);
                            var max = Convert.ToInt32(args[1]);
                            if (min >= max)
                            {
                                error = $"{min} is not < {max} for rand";
                            }
                            else
                            {
                                lock (_randomizerLock)
                                {
                                    value = Randomizer.Next(min, max);
                                }
                            }

                            return (value, error);
                        },
                        VerifyInteger),
                    ReturnType.Number,
                    ValidateBinaryNumber),
                new ExpressionEvaluator(ExpressionType.CreateArray, Apply(args => new List<object>(args)), ReturnType.Object),
                new ExpressionEvaluator(
                    ExpressionType.First,
                    Apply(
                        args =>
                        {
                            object first = null;
                            if (args[0] is string string0 && string0.Length > 0)
                            {
                                first = string0.First().ToString();
                            }
                            else if (TryParseList(args[0], out IList list) && list.Count > 0)
                            {
                                first = AccessIndex(list, 0).value;
                            }

                            return first;
                        }),
                    ReturnType.Object,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.Last,
                    Apply(
                        args =>
                        {
                            object last = null;
                            if (args[0] is string string0 && string0.Length > 0)
                            {
                                last = string0.Last().ToString();
                            }
                            else if (TryParseList(args[0], out IList list) && list.Count > 0)
                            {
                                last = AccessIndex(list, list.Count - 1).value;
                            }

                            return last;
                        }),
                    ReturnType.Object,
                    ValidateUnary),

                // Object manipulation and construction functions
                new ExpressionEvaluator(ExpressionType.Json, Apply(args => JToken.Parse(args[0].ToString())), ReturnType.Object, (expr) => ValidateOrder(expr, null, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.AddProperty,
                    ApplyWithError(args =>
                        {
                            var newJobj = (IDictionary<string, JToken>)args[0];
                            var prop = args[1].ToString();
                            string error = null;
                            if (newJobj.ContainsKey(prop))
                            {
                                error = $"{prop} already exists";
                            }
                            else
                            {
                                newJobj[prop] = JToken.FromObject(args[2]);
                            }

                            return (newJobj, error);
                        }),
                    ReturnType.Object,
                    (expr) => ValidateOrder(expr, null, ReturnType.Object, ReturnType.String, ReturnType.Object)),
                new ExpressionEvaluator(
                    ExpressionType.SetProperty,
                    Apply(args =>
                        {
                            var newJobj = (IDictionary<string, JToken>)args[0];
                            newJobj[args[1].ToString()] = JToken.FromObject(args[2]);

                            return newJobj;
                        }),
                    ReturnType.Object,
                    (expr) => ValidateOrder(expr, null, ReturnType.Object, ReturnType.String, ReturnType.Object)),
                new ExpressionEvaluator(
                    ExpressionType.RemoveProperty,
                    Apply(args =>
                        {
                            var newJobj = (JObject)args[0];
                            newJobj.Property(args[1].ToString()).Remove();
                            return newJobj;
                        }),
                    ReturnType.Object,
                    (expr) => ValidateOrder(expr, null, ReturnType.Object, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.SetPathToValue,
                    SetPathToValue,
                    ReturnType.Object,
                    ValidateBinary),
                new ExpressionEvaluator(ExpressionType.Select, Foreach, ReturnType.Object, ValidateForeach),
                new ExpressionEvaluator(ExpressionType.Foreach, Foreach, ReturnType.Object, ValidateForeach),
                new ExpressionEvaluator(ExpressionType.Where, Where, ReturnType.Object, ValidateWhere),
                new ExpressionEvaluator(ExpressionType.Coalesce, Apply(args => Coalesce(args.ToArray<object>())), ReturnType.Object, ValidateAtLeastOne),
                new ExpressionEvaluator(ExpressionType.XPath, ApplyWithError(args => XPath(args[0], args[1])), ReturnType.Object, (expr) => ValidateOrder(expr, null, ReturnType.Object, ReturnType.String)),
                new ExpressionEvaluator(ExpressionType.JPath, ApplyWithError(args => JPath(args[0], args[1].ToString())), ReturnType.Object, (expr) => ValidateOrder(expr, null, ReturnType.Object, ReturnType.String)),

                // Regex expression
                new ExpressionEvaluator(
                    ExpressionType.IsMatch,
                    ApplyWithError(
                        args =>
                        {
                            var value = false;
                            string error = null;

                            string inputString = args[0]?.ToString();
                            if (string.IsNullOrEmpty(inputString))
                            {
                                value = false;
                                error = "regular expression is empty.";
                            }
                            else
                            {
                                var regex = CommonRegex.CreateRegex(args[1].ToString());
                                value = regex.IsMatch(inputString);
                            }

                            return (value, error);
                        }, VerifyStringOrNull),
                    ReturnType.Boolean,
                    ValidateIsMatch),

                //Type Checking Functions
                new ExpressionEvaluator(
                    ExpressionType.IsString,
                    Apply(args => args[0].GetType() == typeof(string)),
                    ReturnType.Boolean,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.IsInteger,
                    Apply(args => Extensions.IsNumber(args[0]) && Convert.ToDouble(args[0]) % 1 == 0),
                    ReturnType.Boolean,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.IsFloat,
                    Apply(args => Extensions.IsNumber(args[0]) && Convert.ToDouble(args[0]) % 1 != 0),
                    ReturnType.Boolean,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.IsArray,
                    Apply(args => TryParseList(args[0], out IList _)),
                    ReturnType.Boolean,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.IsObject,
                    Apply(args => !(args[0] is JValue) && args[0].GetType().IsValueType == false && args[0].GetType() != typeof(string)),
                    ReturnType.Boolean,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.IsBoolean,
                    Apply(args => args[0] is bool),
                    ReturnType.Boolean,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.IsDateTime,
                    Apply(
                        args => 
                        {
                            if (args[0] is string) 
                            {
                                object value = null;
                                string error = null;
                                (value, error) = ParseISOTimestamp(args[0] as string);
                                if (error == null)
                                {
                                    return true;
                                }
                            }

                            return false;
                        }),
                    ReturnType.Boolean,
                    ValidateUnary),
            };

            var eval = new ExpressionEvaluator(ExpressionType.Optional, (expression, state) => throw new NotImplementedException(), ReturnType.Boolean, ValidateUnaryBoolean);
            eval.Negation = eval;
            functions.Add(eval);
            
            eval = new ExpressionEvaluator(ExpressionType.Ignore, (expression, state) => expression.Children[0].TryEvaluate(state), ReturnType.Boolean, ValidateUnaryBoolean);
            eval.Negation = eval;
            functions.Add(eval);

            var lookup = new Dictionary<string, ExpressionEvaluator>();
            foreach (var function in functions)
            {
                lookup.Add(function.Type, function);
            }

            // Attach negations
            lookup[ExpressionType.LessThan].Negation = lookup[ExpressionType.GreaterThanOrEqual];
            lookup[ExpressionType.LessThanOrEqual].Negation = lookup[ExpressionType.GreaterThan];
            lookup[ExpressionType.Equal].Negation = lookup[ExpressionType.NotEqual];

            // Math aliases
            lookup.Add("add", lookup[ExpressionType.Add]); // more than 1 params
            lookup.Add("div", lookup[ExpressionType.Divide]); // more than 1 params
            lookup.Add("mul", lookup[ExpressionType.Multiply]); // more than 1 params
            lookup.Add("sub", lookup[ExpressionType.Subtract]); // more than 1 params
            lookup.Add("exp", lookup[ExpressionType.Power]); // more than 1 params
            lookup.Add("mod", lookup[ExpressionType.Mod]);

            // Comparison aliases
            lookup.Add("and", lookup[ExpressionType.And]);
            lookup.Add("equals", lookup[ExpressionType.Equal]);
            lookup.Add("greater", lookup[ExpressionType.GreaterThan]);
            lookup.Add("greaterOrEquals", lookup[ExpressionType.GreaterThanOrEqual]);
            lookup.Add("less", lookup[ExpressionType.LessThan]);
            lookup.Add("lessOrEquals", lookup[ExpressionType.LessThanOrEqual]);
            lookup.Add("not", lookup[ExpressionType.Not]);
            lookup.Add("or", lookup[ExpressionType.Or]);

            lookup.Add("&", lookup[ExpressionType.Concat]);
            return new ReadOnlyDictionary<string, ExpressionEvaluator>(lookup);
        }
    }
}
