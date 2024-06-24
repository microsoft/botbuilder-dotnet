// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using Json.More;
using Microsoft.Bot.AdaptiveExpressions.Core.Memory;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace Microsoft.Bot.AdaptiveExpressions.Core
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
            if (!(value is JsonObject) && value is IList listValue)
            {
                list = listValue;
                isList = true;
            }
            else if (value is JsonArray jarray)
            {
                list = jarray.ToList();
                isList = true;
            }

            return isList;
        }

        /// <summary>
        /// Try to coerce object to IList.
        /// </summary>
        /// <param name="value">Value to coerce.</param>
        /// <param name="list">IList if found.</param>
        /// <returns>true if found IList.</returns>
        public static bool TryAsList(object value, out IEnumerable list)
        {
            var isList = false;
            list = null;
            if (value is IList listValue)
            {
                list = listValue;
                isList = true;
            }
            else if (value is JsonArray jarray)
            {
                list = jarray;
                isList = true;
            }

            return isList;
        }

        /// <summary>
        /// Get the count from a List-like thing.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns>The count.</returns>
        /// <exception cref="InvalidOperationException">If the list is not a list.</exception>
        public static int GetListCount(IEnumerable list)
        {
            if (list is IList listValue)
            {
                return listValue.Count;
            }
            else if (list is JsonArray jarray)
            {
                return jarray.Count;
            }

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Get the count from a List-like thing.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="value">The value to append.</param>
        /// <exception cref="InvalidOperationException">If the list is not a list.</exception>
        [RequiresDynamicCode("Uses JsonSerializer on unknown types")]
        [RequiresUnreferencedCode("uses JsonSerializer on unknown types")]
        public static void AppendToList(IEnumerable list, object value)
        {
            if (list is IList ilist)
            {
                ilist.Add(value);
            }
            else if (list is JsonArray jarray)
            {
                JsonValue jvalue = value as JsonValue;
                if (jvalue == null)
                {
                    jvalue = JsonValue.Create(value);
                }

                jarray.Add(jvalue);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Get the count from a List-like thing.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="idx">The index to set.</param>
        /// <param name="value">The new value.</param>
        /// <exception cref="InvalidOperationException">If the list is not a list.</exception>
        [RequiresDynamicCode("SetIndex uses JsonSerializer on unknown types")]
        [RequiresUnreferencedCode("SetIndex uses JsonSerializer on unknown types")]
        public static void SetIndex(IEnumerable list, int idx, object value)
        {
            if (list is IList ilist)
            {
                ilist[idx] = value;
            }
            else if (list is JsonArray jarray)
            {
                JsonValue jvalue = value as JsonValue;
                if (jvalue == null)
                {
                    jvalue = JsonValue.Create(value);
                }

                jarray[idx] = jvalue;
            }
            else
            {
                throw new InvalidOperationException();
            }
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
        public static EvaluateExpressionDelegate Apply(Func<IReadOnlyList<object>, IMemory, object> function, VerifyExpression verify = null)
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
                        value = function(args, state);
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
        public static EvaluateExpressionDelegate ApplyWithError(Func<IReadOnlyList<object>, IMemory, (object, string)> function, VerifyExpression verify = null)
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
                        (value, error) = function(args, state);
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
        /// Generate an expression delegate that applies function after verifying all children.
        /// </summary>
        /// <param name="function">Function to apply.</param>
        /// <param name="verify">Function to check each arg for validity.</param>
        /// <returns>Delegate for evaluating an expression.</returns>
        public static EvaluateExpressionDelegate ApplyWithOptionsAndError(Func<IReadOnlyList<object>, IMemory, Options, (object, string)> function, VerifyExpression verify = null)
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
                        (value, error) = function(args, state, options);
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
            path = path.TrimEnd('.').Replace(".[", "[", StringComparison.Ordinal);

            if (string.IsNullOrEmpty(path))
            {
                path = null;
            }

            return (path, left, null);
        }

        /// <summary>
        /// Judge if two objects are equal.
        /// </summary>
        /// <param name="obj1">First object.</param>
        /// <param name="obj2">Second object.</param>
        /// <param name="state">IMemory to delegate serialization to if needed.</param>
        /// <returns>If two objects are equal.</returns>
        public static bool CommonEquals(object obj1, object obj2, IMemory state)
        {
            if (obj1 == null || obj2 == null)
            {
                // null will only equals to null
                return obj1 == null && obj2 == null;
            }

            if (obj1 is JsonValue jobj1 && obj2 is JsonValue jobj2)
            {
                return JsonValue.DeepEquals(jobj1, jobj2);
            }

            obj1 = ResolveValue(obj1);
            obj2 = ResolveValue(obj2);

            // Array Comparison
            if (TryParseList(obj1, out IList l0) && TryParseList(obj2, out IList l1))
            {
                if (l0.Count != l1.Count)
                {
                    return false;
                }

                var isEqual = true;
                for (var i = 0; i < l0.Count; i++)
                {
                    if (!CommonEquals(l0[i], l1[i], state))
                    {
                        isEqual = false;
                        break;
                    }
                }

                return isEqual;
            }

            // Object Comparison
            var propertyCountOfObj1 = GetPropertyCount(obj1);
            var propertyCountOfObj2 = GetPropertyCount(obj2);
            if (propertyCountOfObj1 >= 0 && propertyCountOfObj2 >= 0)
            {
                if (propertyCountOfObj1 != propertyCountOfObj2)
                {
                    return false;
                }

                var jObj1 = state.SerializeToNode(obj1);
                var jObj2 = state.SerializeToNode(obj2);
                return JsonNode.DeepEquals(jObj1, jObj2);
            }

            // Number Comparison
            if (obj1.IsNumber() && obj2.IsNumber())
            {
                if (Math.Abs(CultureInvariantDoubleConvert(obj1) - CultureInvariantDoubleConvert(obj2)) < double.Epsilon)
                {
                    return true;
                }
            }

            try
            {
                return obj1 == obj2 || (obj1 != null && obj1.Equals(obj2));
            }
#pragma warning disable CA1031 // Do not catch general exception types (we return false if it fails for whatever reason)
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                return false;
            }
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
        /// Lookup a property in IDictionary, JsonObject or through reflection.
        /// </summary>
        /// <param name="instance">Instance with property.</param>
        /// <param name="property">Property to lookup.</param>
        /// <param name="value">Value of property.</param>
        /// <returns>True if property is present and binds value.</returns>
        [UnconditionalSuppressMessage("Trimming", "IL2075:'this' argument does not satisfy 'DynamicallyAccessedMemberTypes.PublicProperties' in call to 'System.Type.GetProperties()'. The return value of method 'System.Object.GetType()' does not have matching annotations. The source value must declare at least the same requirements as those declared on the target location it is assigned to.", Justification = "AOT aware callers will not go through reflection path")]
        internal static bool TryAccessProperty(object instance, string property, out object value)
        {
            var isPresent = false;
            value = null;
            if (instance != null)
            {
                // NOTE: what about other type of TKey, TValue?
                if (instance is IDictionary<string, object> idict)
                {
                    if (!idict.TryGetValue(property, out value))
                    {
                        // fall back to case insensitive
                        var prop = idict.Keys.Where(k => string.Equals(k, property, StringComparison.OrdinalIgnoreCase)).SingleOrDefault();
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
                else if (instance is IDictionary<string, JsonNode> jdict)
                {
                    // This case covers JsonObject as well as the result of IndicesAndValues' listification
                    var result = jdict.FirstOrDefault(x => x.Key.Equals(property, StringComparison.OrdinalIgnoreCase));
                    value = result.Value;
                    isPresent = result.Key != null;
                }
                else
                {
                    // Use reflection
                    var type = instance.GetType();
                    var prop = type.GetProperties().Where(p => string.Equals(p.Name, property, StringComparison.OrdinalIgnoreCase)).SingleOrDefault();
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
        /// Convert constant JsonValue to base type value.
        /// </summary>
        /// <param name="obj">Input object.</param>
        /// <returns>Corresponding base type if input is a JsonValue.</returns>
        internal static object ResolveValue(object obj)
        {
            if (obj is JsonValue jval)
            {
                switch (jval.GetValueKind())
                {
                    case JsonValueKind.String:
                        return jval.GetString();
                    case JsonValueKind.Number:
                        // If this is a JsonElement we can get back int vs floating point from it, so try that first
                        if (jval.TryGetValue<JsonElement>(out var jelem))
                        {
                            return ResolveValue(jelem);
                        }

                        return jval.GetNumber();
                    case JsonValueKind.Null:
                        return null;
                    case JsonValueKind.True:
                        return true;
                    case JsonValueKind.False:
                        return false;
                    default:
                        return jval.GetValue<object>();
                }
            }
            else if (obj is JsonElement jelem)
            {
                return ResolveValue(jelem);
            }

            return obj;
        }

        internal static object ResolveValue(JsonElement jelem)
        {
            switch (jelem.ValueKind)
            {
                case JsonValueKind.String:
                    return jelem.GetString();
                case JsonValueKind.Number:
                    if (jelem.TryGetInt32(out var int32))
                    {
                        return int32;
                    }
                    else if (jelem.TryGetInt64(out var int64))
                    {
                        return int64;
                    }

                    return jelem.GetDecimal();
                case JsonValueKind.Null:
                    return null;
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                default:
                    Environment.FailFast("Unhandled JsonValueKind");
                    return null;
            }
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
        /// Return new object list replace JsonArray.ToArray&lt;object&gt;().
        /// </summary>
        /// <param name="instance">List to resolve.</param>
        /// <returns>Resolved list.</returns>
        internal static IList ResolveListValue(object instance)
        {
            IList result = null;
            if (instance is JsonArray ja)
            {
                result = ja.Select(x => ResolveValue(x)).ToList();
            }
            else if (TryParseList(instance, out var list))
            {
                result = new List<object>();
                foreach (var x in list)
                {
                    result.Add(ResolveValue(x));
                }
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
                var list = ConvertToList(instance, state);
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

                // the local iterator is pushed as one memory layer in the memory stack
                stackedMemory.PushLocalIterator(iteratorName, currentItem);
                (var r, var e) = expression.Children[2].TryEvaluate(stackedMemory, options);
                stackedMemory.Pop();

                var shouldBreak = callback(currentItem, r, e);
                if (shouldBreak)
                {
                    break;
                }
            }
        }

        internal static IList ConvertToList(object instance, IMemory state)
        {
            IList list = null;
            if (TryParseList(instance, out IList ilist))
            {
                list = ilist;
            }
            else if (instance is JsonObject jobj)
            {
                list = Object2KVPairList(jobj);
            }
            else if (state.SerializeToNode(instance) is JsonObject jsonObject)
            {
                list = Object2KVPairList(jsonObject);
            }

            return list;
        }

        internal static List<object> Object2KVPairList(JsonObject jobj)
        {
            return jobj.ToList().ConvertAll<object>(
                x => new Dictionary<string, JsonNode>
                    {
                        { "key", x.Key },
                        { "value", x.Value }
                    });
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
            else if (timexExpr is JsonObject jTimex)
            {
                parsed = jTimex.Deserialize(AdaptiveExpressionsSerializerContext.Default.TimexProperty);
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

        // collection functions

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "AOT aware callers will not need us to call JsonSerializer")]
        [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "AOT aware callers will not need us to call JsonSerializer")]
        internal static EvaluateExpressionDelegate SortBy(bool isDescending)
           => (expression, state, options) =>
           {
               object result = null;
               string error;
               object arr;
               (arr, error) = expression.Children[0].TryEvaluate(state, options);

               if (error == null)
               {
                   if (TryAsList(arr, out var list))
                   {
                       if (expression.Children.Length == 1)
                       {
                           if (isDescending)
                           {
                               result = list.OfType<object>().OrderByDescending(item => item, JsonObjectComparer.Instance).ToList();
                           }
                           else
                           {
                               result = list.OfType<object>().OrderBy(item => item, JsonObjectComparer.Instance).ToList();
                           }
                       }
                       else
                       {
                           JsonArray jsonArray = list as JsonArray;
                           if (jsonArray == null)
                           {
                               jsonArray = JsonSerializer.SerializeToNode(list).AsArray();
                           }

                           var propertyNameExpression = expression.Children[1];
                           string propertyName;
                           (propertyName, error) = propertyNameExpression.TryEvaluate<string>(state, options);
                           if (error == null)
                           {
                               propertyName = propertyName ?? string.Empty;
                               if (isDescending)
                               {
                                   result = jsonArray.OrderByDescending(obj => obj[propertyName], JsonObjectComparer.Instance).ToList();
                               }
                               else
                               {
                                   result = jsonArray.OrderBy(obj => obj[propertyName], JsonObjectComparer.Instance).ToList();
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

        internal static void Merge(this JsonObject target, JsonObject source)
        {
            foreach (var kvp in source)
            {
                var newValue = kvp.Value;
                if (target.TryGetPropertyValue(kvp.Key, out var value))
                {
                    if (value.GetValueKind() == JsonValueKind.Object && newValue.GetValueKind() == JsonValueKind.Object)
                    {
                        newValue.AsObject().Merge(value.AsObject());
                    }
                    else if (value.GetValueKind() == JsonValueKind.Array && newValue.GetValueKind() == JsonValueKind.Array)
                    {
                        // Merge strategy = replace, so ignore the existing array.
                        //newValue.AsArray().Merge(value.AsArray());
                    }
                }

                target[kvp.Key] = newValue.DeepClone();
            }
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "AOT caller will have supplied a non-null JsonTypeInfo")]
        [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "AOT caller will have supplied a non-null JsonTypeInfo")]
        internal static void SerializeValueToWriter<T>(Utf8JsonWriter writer, T value, JsonTypeInfo valueJsonTypeInfo, JsonSerializerOptions options)
        {
            if (valueJsonTypeInfo != null)
            {
                JsonSerializer.Serialize(writer, value, valueJsonTypeInfo);
            }
            else
            {
                JsonValue.Create(value).WriteTo(writer, options);
            }
        }

        private static (object, string) ParseISOTimestamp(string timeStamp, Func<DateTime, (object, string)> transform = null)
        {
            object result = null;
            string error = null;

            try
            {
                var parsed = JsonSerializer.Deserialize($"\"{timeStamp}\"", AdaptiveExpressionsSerializerContext.Default.DateTime);

                (result, error) = transform != null ? transform(parsed) : (parsed, null);
            }
            catch (JsonException)
            {
                error = $"{timeStamp} is not standard ISO format.";
            }

            return (result, error);
        }

        private static string BuildTypeValidatorError(ReturnType returnType, Expression childExpr, Expression expr)
        {
            string result;
            var names = returnType.ToString();
            if (!names.Contains(",", StringComparison.Ordinal))
            {
                result = $"{childExpr} is not a {names} expression in {expr}.";
            }
            else
            {
                result = $"{childExpr} in {expr} is not any of [{names}].";
            }

            return result;
        }

        [UnconditionalSuppressMessage("Trimming", "IL2075:'this' argument does not satisfy 'DynamicallyAccessedMemberTypes.PublicProperties' in call to 'System.Type.GetProperties()'. The return value of method 'System.Object.GetType()' does not have matching annotations. The source value must declare at least the same requirements as those declared on the target location it is assigned to.", Justification = "AOT aware callers will not go through reflection path")]
        private static int GetPropertyCount(object obj)
        {
            if (obj is IDictionary dictionary)
            {
                return dictionary.Count;
            }
            else if (obj is JsonObject jobj)
            {
                return jobj.Count;
            }
            else if (!(obj is JsonValue) && obj.GetType().IsValueType == false && obj.GetType().FullName != "System.String")
            {
                // exclude constant type.
                return obj.GetType().GetProperties().Length;
            }

            return -1;
        }

        private class JsonObjectComparer : IComparer<JsonNode>, IComparer<object>
        {
            public static readonly JsonObjectComparer Instance = new JsonObjectComparer();

            public int Compare(JsonNode x, JsonNode y)
            {
                var xkind = x.GetValueKind();
                var ykind = y.GetValueKind();
                if (xkind == ykind)
                {
                    if (xkind == JsonValueKind.Number)
                    {
                        return x.AsValue().GetValue<decimal>().CompareTo(y.AsValue().GetValue<decimal>());
                    }
                    else if (xkind == JsonValueKind.String)
                    {
                        return string.CompareOrdinal(x.AsValue().GetString(), y.AsValue().GetString());
                    }
                }

                return xkind.CompareTo(ykind);
            }

            public int Compare(object x, object y)
            {
                if (x is JsonNode xj && y is JsonNode yj)
                {
                    return Compare(xj, yj);
                }
                else if (x is string xs && y is string ys)
                {
                    return string.CompareOrdinal(xs, ys);
                }
                else if (x is IComparable xc && y is IComparable yc)
                {
                    return xc.CompareTo(yc);
                }

                if (x == null && y == null)
                {
                    return 0;
                }
                else if (x == null && y != null)
                {
                    return -1;
                }
                else if (x != null && y == null)
                {
                    return 1;
                }

                return x.GetHashCode().CompareTo(y.GetHashCode());
            }
        }
    }
}
