// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using AdaptiveExpressions.Memory;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions
{
    /// <summary>
    /// Utility functions for Adaptive-Expressions.
    /// </summary>
    public static class FunctionUtils
    {
        /// <summary>
        /// The default date time format string.
        /// </summary>
        public static readonly string DefaultDateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";

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
        /// <param name="returnType">Allowed return types for children.</param>
        public static void ValidateArityAndAnyType(Expression expression, int minArity, int maxArity, ReturnType returnType = ReturnType.Object)
        {
            if (expression.Children.Length < minArity)
            {
                throw new ArgumentException($"{expression} should have at least {minArity} children.");
            }

            if (expression.Children.Length > maxArity)
            {
                throw new ArgumentException($"{expression} can't have more than {maxArity} children.");
            }

            if ((returnType & ReturnType.Object) == 0)
            {
                foreach (var child in expression.Children)
                {
                    if ((child.ReturnType & ReturnType.Object) == 0 && (returnType & child.ReturnType) == 0)
                    {
                        throw new ArgumentException(BuildTypeValidatorError(returnType, child, expression));
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
                optional = Array.Empty<ReturnType>();
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
                if ((type & ReturnType.Object) == 0
                    && (child.ReturnType & ReturnType.Object) == 0
                    && (type & child.ReturnType) == 0)
                {
                    throw new ArgumentException(BuildTypeValidatorError(type, child, expression));
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
                if ((type & ReturnType.Object) == 0
                    && (child.ReturnType & ReturnType.Object) == 0
                    && (type & child.ReturnType) == 0)
                {
                    throw new ArgumentException(BuildTypeValidatorError(type, child, expression));
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
        /// Validate 1 or 2 numeric arguments.
        /// </summary>
        /// <param name="expression">Expression to validate.</param>
        public static void ValidateUnaryOrBinaryNumber(Expression expression)
            => ValidateArityAndAnyType(expression, 1, 2, ReturnType.Number);

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
            => ValidateArityAndAnyType(expression, 2, 2, ReturnType.Number | ReturnType.String);

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
        /// Validate 1 or 2 string arguments.
        /// </summary>
        /// <param name="expression">Expression to validate.</param>
        public static void ValidateUnaryOrBinaryString(Expression expression)
            => ValidateArityAndAnyType(expression, 1, 2, ReturnType.String);

        /// <summary>
        /// Validate there is a single number argument.
        /// </summary>
        /// <param name="expression">Expression to validate.</param>
        public static void ValidateUnaryNumber(Expression expression)
        => ValidateArityAndAnyType(expression, 1, 1, ReturnType.Number);

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
#pragma warning disable CA1801 // Review unused parameters (we can't remove the number parameter without breaking backward compat)
        public static string VerifyNumber(object value, Expression expression, int number)
#pragma warning restore CA1801 // Review unused parameters
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
#pragma warning disable CA1801 // Review unused parameters (we can't remove the number parameter without breaking binary compat)
        public static string VerifyNumericList(object value, Expression expression, int number)
#pragma warning restore CA1801 // Review unused parameters
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
#pragma warning disable CA1801 // Review unused parameters (we can't remove the number parameter without breaking binary compat)
        public static string VerifyNumericListOrNumber(object value, Expression expression, int number)
#pragma warning restore CA1801 // Review unused parameters
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
#pragma warning disable CA1801 // Review unused parameters (we can't remove the number parameter without breaking binary compat)
        public static string VerifyContainer(object value, Expression expression, int number)
#pragma warning restore CA1801 // Review unused parameters
        {
            string error = null;
            if (!(value is string) && !(value is IList) && !(value is IEnumerable))
            {
                error = $"{expression} must be a string or list.";
            }

            return error;
        }

        /// <summary>
        /// Verify value contains elements or null.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="expression">Expression that led to value.</param>
        /// <param name="number">No function.</param>
        /// <returns>Error or null if valid.</returns>
#pragma warning disable CA1801 // Review unused parameters (we can't remove the number parameter without breaking binary compat)
        public static string VerifyContainerOrNull(object value, Expression expression, int number)
#pragma warning restore CA1801 // Review unused parameters
        {
            string error = null;
            if (value != null && !(value is string) && !(value is IList) && !(value is IEnumerable))
            {
                error = $"{expression} must be a string or list or a null object.";
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
#pragma warning disable CA1801 // Review unused parameters (we can't remove the number parameter without breaking binary compat)
        public static string VerifyList(object value, Expression expression, int number)
#pragma warning restore CA1801 // Review unused parameters
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
#pragma warning disable CA1801 // Review unused parameters (we can't remove the number parameter without breaking binary compat)
        public static string VerifyInteger(object value, Expression expression, int number)
#pragma warning restore CA1801 // Review unused parameters
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
#pragma warning disable CA1801 // Review unused parameters (we can't remove the number parameter without breaking binary compat)
        public static string VerifyString(object value, Expression expression, int number)
#pragma warning restore CA1801 // Review unused parameters
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
#pragma warning disable CA1801 // Review unused parameters (we can't remove the number parameter without breaking binary compat)
        public static string VerifyStringOrNull(object value, Expression expression, int number)
#pragma warning restore CA1801 // Review unused parameters
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
#pragma warning disable CA1801 // Review unused parameters (we can't remove the number parameter without breaking binary compat)
        public static string VerifyNotNull(object value, Expression expression, int number)
#pragma warning restore CA1801 // Review unused parameters
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
#pragma warning disable CA1801 // Review unused parameters (we can't remove the number parameter without breaking binary compat)
        public static string VerifyNumberOrString(object value, Expression expression, int number)
#pragma warning restore CA1801 // Review unused parameters
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
#pragma warning disable CA1801 // Review unused parameters (we can't remove the number parameter without breaking binary compat)
        public static string VerifyNumberOrStringOrNull(object value, Expression expression, int number)
#pragma warning restore CA1801 // Review unused parameters
        {
            string error = null;
            if (value != null && !value.IsNumber() && !(value is string))
            {
                error = $"{expression} is not string or number.";
            }

            return error;
        }

        /// <summary>
        /// Evaluate expression children and return them.
        /// </summary>
        /// <param name="expression">Expression with children.</param>
        /// <param name="state">Global state.</param>
        /// <param name="options">Options used in evaluation. </param>
        /// <param name="verify">Optional function to verify each child's result.</param>
        /// <returns>List of child values or error message.</returns>
        public static (IReadOnlyList<object>, string error) EvaluateChildren(Expression expression, IMemory state, Options options, VerifyExpression verify = null)
        {
            var args = new List<object>();
            object value;
            string error = null;
            var pos = 0;
            foreach (var child in expression.Children)
            {
                (value, error) = child.TryEvaluate(state, options);
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
            (expression, state, options) =>
            {
                object value = null;
                string error = null;
                IReadOnlyList<object> args;
                (args, error) = EvaluateChildren(expression, state, options, verify);
                if (error == null)
                {
                    try
                    {
                        value = function(args);
                    }
#pragma warning disable CA1031 // Do not catch general exception types (capture any exception and return it in the error)
                    catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
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
            (expression, state, options) =>
            {
                object value = null;
                string error = null;
                IReadOnlyList<object> args;
                (args, error) = EvaluateChildren(expression, state, options, verify);
                if (error == null)
                {
                    try
                    {
                        (value, error) = function(args);
                    }
#pragma warning disable CA1031 // Do not catch general exception types (capture any exception and return it in the error)
                    catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
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
        public static EvaluateExpressionDelegate ApplyWithOptionsAndError(Func<IReadOnlyList<object>, Options, (object, string)> function, VerifyExpression verify = null)
            =>
            (expression, state, options) =>
            {
                object value = null;
                string error = null;
                IReadOnlyList<object> args;
                (args, error) = EvaluateChildren(expression, state, options, verify);
                if (error == null)
                {
                    try
                    {
                        (value, error) = function(args, options);
                    }
#pragma warning disable CA1031 // Do not catch general exception types (caputure any exception which may happen in the delegate function and return it)
                    catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
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
        /// Try to accumulate the path from an Accessor or Element, from right to left.
        /// </summary>
        /// <param name="expression">expression.</param>
        /// <param name="state">scope.</param>
        /// <param name="options">Options used in evaluation. </param>
        /// <returns>return the accumulated path and the expression left unable to accumulate.</returns>
        public static (string path, Expression left, string error) TryAccumulatePath(Expression expression, IMemory state, Options options)
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
                    var (value, error) = left.Children[1].TryEvaluate(state, options);
                    if (error != null)
                    {
                        return (null, null, error);
                    }

                    if (value.IsInteger())
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

        /// <summary>
        /// Lookup an index property of instance.
        /// </summary>
        /// <param name="instance">Instance with property.</param>
        /// <param name="index">Property to lookup.</param>
        /// <returns>Value and error information if any.</returns>
        internal static (object value, string error) AccessIndex(object instance, long index)
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
                    var newIndex = 0;
                    (newIndex, error) = ParseInt32(index);
                    if (error == null)
                    {
                        value = list[newIndex];
                    }
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

        internal static (object, string) TicksWithError(object timestamp)
        {
            object result = null;
            object parsed = null;
            string error = null;
            (parsed, error) = NormalizeToDateTime(timestamp);

            if (error == null)
            {
                var ts = (DateTime)parsed;
                result = ts.Ticks;
            }

            return (result, error);
        }

        /// <summary>
        /// Lookup a property in IDictionary, JObject or through reflection.
        /// </summary>
        /// <param name="instance">Instance with property.</param>
        /// <param name="property">Property to lookup.</param>
        /// <param name="value">Value of property.</param>
        /// <returns>True if property is present and binds value.</returns>
        internal static bool TryAccessProperty(object instance, string property, out object value)
        {
            var isPresent = false;
            value = null;
            if (instance != null)
            {
                property = property.ToLowerInvariant();

                // NOTE: what about other type of TKey, TValue?
                if (instance is IDictionary<string, object> idict)
                {
                    if (!idict.TryGetValue(property, out value))
                    {
                        // fall back to case insensitive
                        var prop = idict.Keys.Where(k => k.ToLowerInvariant() == property).SingleOrDefault();
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
                    var prop = type.GetProperties().Where(p => p.Name.ToLowerInvariant() == property).SingleOrDefault();
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
        /// Convert an input object to 32-bit signed interger. If failed, an error messgage will returned.
        /// </summary>
        /// <param name="obj">Input object.</param>
        /// <returns>A tuple of an integer and a string.</returns>
        internal static (int, string) ParseInt32(object obj)
        {
            int result = 0;
            string error = null;
            try
            {
                result = Convert.ToInt32(obj, CultureInfo.InvariantCulture);
            }
#pragma warning disable CA1031 // Do not catch general exception types (capture any exception and return generic message)
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                error = $"{obj} must be a 32-bit signed integer.";
            }

            return (result, error);
        }

        /// <summary>
        /// Convert constant JValue to base type value.
        /// </summary>
        /// <param name="obj">Input object.</param>
        /// <returns>Corresponding base type if input is a JValue.</returns>
        internal static object ResolveValue(object obj)
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
                    value = jval.ToObject<long>();
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
                    value = jval.ToObject<double>();
                }
            }

            return value;
        }

        internal static (object value, string error) WrapGetValue(IMemory memory, string property, Options options)
        {
            if (memory.TryGetValue(property, out var result) && result != null)
            {
                return (result, null);
            }

            if (options.NullSubstitution != null)
            {
                return (options.NullSubstitution(property), null);
            }

            return (null, null);
        }

        internal static string ParseStringOrNull(object value)
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
        internal static IList ResolveListValue(object instance)
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

        /// <summary>
        /// Test result to see if True in logical comparison functions.
        /// </summary>
        /// <param name="instance">Computed value.</param>
        /// <returns>True if boolean true or non-null.</returns>
        internal static bool IsLogicTrue(object instance)
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

        internal static double CultureInvariantDoubleConvert(object numberObj) => Convert.ToDouble(numberObj, CultureInfo.InvariantCulture);

        internal static (object value, string error) Foreach(Expression expression, IMemory state, Options options)
        {
            object result = null;
            string error;

            object instance;
            (instance, error) = expression.Children[0].TryEvaluate(state, options);
            if (instance == null)
            {
                error = $"'{expression.Children[0]}' evaluated to null.";
            }
            else if (error == null)
            {
                var list = ConvertToList(instance);
                if (list == null)
                {
                    error = $"{expression.Children[0]} is not a collection or structure object to run Foreach";
                }
                else
                {
                    result = new List<object>();
                    LambdaEvaluator(expression, state, options, list, (object currentItem, object r, string e) =>
                    {
                        if (e != null)
                        {
                            error = e;
                            return true;
                        }
                        else
                        {
                            ((List<object>)result).Add(r);
                            return false;
                        }
                    });
                }
            }

            return (result, error);
        }

        internal static void LambdaEvaluator(Expression expression, IMemory state, Options options, IList list, Func<object, object, string, bool> callback)
        {
            var iteratorName = (string)(expression.Children[1].Children[0] as Constant).Value;
            var stackedMemory = StackedMemory.Wrap(state);
            for (var idx = 0; idx < list.Count; idx++)
            {
                var currentItem = AccessIndex(list, idx).value;
                var local = new Dictionary<string, object>
                {
                    { iteratorName, currentItem },
                };

                // the local iterator is pushed as one memory layer in the memory stack
                stackedMemory.Push(new SimpleObjectMemory(local));
                (var r, var e) = expression.Children[2].TryEvaluate(stackedMemory, options);
                stackedMemory.Pop();

                var shouldBreak = callback(currentItem, r, e);
                if (shouldBreak)
                {
                    break;
                }
            }
        }

        internal static IList ConvertToList(object instance)
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
            else if (ConvertToJToken(instance) is JObject jobject)
            {
                list = Object2KVPairList(jobject);
            }

            return list;
        }

        internal static List<object> Object2KVPairList(JObject jobj)
        {
            var tempList = new List<object>();
            foreach (var item in jobj)
            {
                tempList.Add(new { key = item.Key, value = item.Value });
            }

            return tempList;
        }

        internal static void ValidateLambdaExpression(Expression expression)
        {
            if (expression.Children.Length != 3)
            {
                throw new ArgumentException($"Lambda expression expects 3 parameters, found {expression.Children.Length}");
            }

            var second = expression.Children[1];

            if (!(second.Type == ExpressionType.Accessor && second.Children.Length == 1))
            {
                throw new ArgumentException($"Second parameter is not an identifier : {second}");
            }
        }

        internal static (Func<DateTime, DateTime>, string) DateTimeConverter(long interval, string timeUnit, bool isPast = true)
        {
            Func<DateTime, DateTime> converter = (dateTime) => dateTime;
            string error = null;
            var multiFlag = isPast ? -1 : 1;
            switch (timeUnit.ToLowerInvariant())
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

        internal static (object, string) NormalizeToDateTime(object timestamp, Func<DateTime, (object, string)> transform = null)
        {
            object result = null;
            string error = null;
            if (timestamp is string ts)
            {
                (result, error) = ParseISOTimestamp(ts, transform);
            }
            else if (timestamp is DateTime dt)
            {
                (result, error) = transform != null ? transform(dt) : (dt, null);
            }
            else
            {
                error = $"{timestamp} should be a standard ISO format string or a DateTime object.";
            }

            return (result, error);
        }

        internal static (CultureInfo, string) TryParseLocale(string locale)
        {
            CultureInfo result = null;
            string error = null;
            try
            {
                result = new CultureInfo(locale);
            }
            catch (CultureNotFoundException e)
            {
                error = e.Message;
            }

            return (result, error);
        }

        internal static (string, CultureInfo, string) DetermineFormatAndLocale(IReadOnlyList<object> args, string format, CultureInfo locale, int maxArgsLength)
        {
            string error = null;
            if (maxArgsLength >= 2)
            {
                if (args.Count == maxArgsLength)
                {
                    // if the number of args equals to the maxArgsLength, the second last one is format, and the last one is locale
                    format = args[maxArgsLength - 2] as string;
                    (locale, error) = TryParseLocale(args[maxArgsLength - 1] as string);
                }
                else if (args.Count == maxArgsLength - 1)
                {
                    // if the number of args equals to the maxArgsLength - 1, the last one is format,
                    format = args[maxArgsLength - 2] as string;
                }
            }

            return (format, locale, error);
        }

        internal static (CultureInfo, string) DetermineLocale(IReadOnlyList<object> args, CultureInfo locale, int maxArgsLength)
        {
            string error = null;
            if (maxArgsLength >= 2)
            {
                // if the number of args equals to the maxArgsLength, the last one is locale
                if (args.Count == maxArgsLength)
                {
                    if (args[maxArgsLength - 1] is string)
                    {
                        (locale, error) = TryParseLocale(args[maxArgsLength - 1] as string);
                    } 
                    else
                    {
                        error = $"{args[maxArgsLength - 1]} should be a locale string.";
                    }
                }
            }

            return (locale, error);
        }

        internal static (object, string) ConvertTimeZoneFormat(string timezone)
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
#pragma warning disable CA1031 // Do not catch general exception types (capture any exception and return generic message)
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                error = $"{timezone} is an illegal timezone";
            }

            return (convertedTimeZone, error);
        }

        internal static (string, string) ReturnFormatTimeStampStr(DateTime datetime, string format, CultureInfo locale)
        {
            string result = null;
            string error = null;
            try
            {
                result = datetime.ToString(format, locale);
            }
#pragma warning disable CA1031 // Do not catch general exception types (capture any exception and return generic message)
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                error = $"illegal format representation: {format}";
            }

            return (result, error);
        }

        // URI Parsing Functions
        internal static (object, string) ParseUri(string uri)
        {
            object result = null;
            string error = null;
            try
            {
                result = new Uri(uri);
            }
#pragma warning disable CA1031 // Do not catch general exception types (capture any exception and return generic message)
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                error = $"{uri} is an illegal URI string";
            }

            return (result, error);
        }

        internal static (TimexProperty, string) ParseTimexProperty(object timexExpr)
        {
            TimexProperty parsed = null;
            string error = null;
            if (timexExpr is TimexProperty timex)
            {
                parsed = timex;
            }
            else if (timexExpr is JObject jTimex)
            {
                parsed = jTimex.ToObject<TimexProperty>();
            }
            else if (timexExpr is string ts)
            {
                parsed = new TimexProperty(ts);
            }
            else
            {
                error = $"{timexExpr} requires a TimexProperty or a string as a argument";
            }

            return (parsed, error);
        }

        // conversion functions
        internal static byte[] ToBinary(string strToConvert)
        {
            if (strToConvert == null)
            {
                return Array.Empty<byte>();
            }

            return Encoding.UTF8.GetBytes(strToConvert);
        }

        internal static JToken ConvertToJToken(object value)
        {
            return value == null ? JValue.CreateNull() : JToken.FromObject(value);
        }

        // collection functions

        internal static EvaluateExpressionDelegate SortBy(bool isDescending)
           => (expression, state, options) =>
           {
               object result = null;
               string error;
               object arr;
               (arr, error) = expression.Children[0].TryEvaluate(state, options);

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
                           (propertyName, error) = propertyNameExpression.TryEvaluate<string>(state, options);
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

        private static (object, string) ParseISOTimestamp(string timeStamp, Func<DateTime, (object, string)> transform = null)
        {
            object result = null;
            string error = null;

            if (DateTime.TryParse(
                    s: timeStamp,
                    provider: CultureInfo.InvariantCulture,
                    styles: DateTimeStyles.RoundtripKind,
                    result: out var parsed))
            {
                if (parsed.ToString(DefaultDateTimeFormat, CultureInfo.InvariantCulture).Equals(timeStamp, StringComparison.OrdinalIgnoreCase))
                {
                    (result, error) = transform != null ? transform(parsed) : (parsed, null);
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

        private static string BuildTypeValidatorError(ReturnType returnType, Expression childExpr, Expression expr)
        {
            string result;
            var names = returnType.ToString();
            if (!names.Contains(","))
            {
                result = $"{childExpr} is not a {names} expression in {expr}.";
            }
            else
            {
                result = $"{childExpr} in {expr} is not any of [{names}].";
            }

            return result;
        }
    }
}
