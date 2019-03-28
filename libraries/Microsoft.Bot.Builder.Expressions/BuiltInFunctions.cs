// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Expressions
{
    /// <summary>
    /// Definition of default built-in functions for expressions.
    /// </summary>
    public static class BuiltInFunctions
    {
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
        public static void ValidateArityAndAnyType(Expression expression, int minArity, int maxArity, params ExpressionReturnType[] types)
        {
            if (expression.Children.Length < minArity)
            {
                throw new ArgumentException($"{expression} should have at least {minArity} children.");
            }
            if (expression.Children.Length > maxArity)
            {
                throw new ArgumentException($"{expression} can't have more than {maxArity} children.");
            }
            foreach (var child in expression.Children)
            {
                if (child.ReturnType != ExpressionReturnType.Object && !types.Contains(child.ReturnType))
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

        /// <summary>
        /// Validate the number and type of arguments to a function.
        /// </summary>
        /// <param name="expression">Expression to validate.</param>
        /// <param name="types">Expected types in order.</param>
        public static void ValidateOrder(Expression expression, params ExpressionReturnType[] types)
        {
            if (expression.Children.Length != types.Count())
            {
                throw new ArgumentException($"{expression} should have {types.Count()} children");
            }
            for (var i = 0; i < expression.Children.Length; ++i)
            {
                var child = expression.Children[i];
                var type = types[i];
                if (child.ReturnType != type)
                {
                    throw new ArgumentException($"{child} in {expression} is not a {type}.");
                }
            }
        }

        /// <summary>
        /// Validate 1 or more numeric arguments.
        /// </summary>
        /// <param name="expression">Expression to validate.</param>
        public static void ValidateNumber(Expression expression)
            => ValidateArityAndAnyType(expression, 1, int.MaxValue, ExpressionReturnType.Number);

        /// <summary>
        /// Validate 1 or more boolean arguments.
        /// </summary>
        /// <param name="expression">Expression to validate.</param>
        public static void ValidateBoolean(Expression expression)
            => ValidateArityAndAnyType(expression, 1, int.MaxValue, ExpressionReturnType.Boolean);

        /// <summary>
        /// Validate there are 2 numeric or string arguments.
        /// </summary>
        /// <param name="expression">Expression to validate.</param>
        public static void ValidateBinaryNumberOrString(Expression expression)
            => ValidateArityAndAnyType(expression, 2, 2, ExpressionReturnType.Number, ExpressionReturnType.String);

        // Verifiers do runtime error checking of expression evaluation

        /// <summary>
        /// Verify value is numeric.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="expression">Expression that led to value.</param>
        /// <returns>Error or null if valid.</returns>
        public static string VerifyNumber(object value, Expression expression)
        {
            string error = null;
            if (!value.IsNumber())
            {
                error = $"{expression} is not a number.";
            }
            return error;
        }

        /// <summary>
        /// Verify value is a number or string.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="expression">Expression that led to value.</param>
        /// <returns>Error or null if valid.</returns>
        public static string VerifyNumberOrString(object value, Expression expression)
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
            if (!(value is Boolean))
            {
                error = $"{expression} is not a boolean.";
            }
            return error;
        }

        // Apply -- these are helpers for adding functions to the expression library.

        /// <summary>
        /// Generate an expression delegate that applies function after verifying all children.
        /// </summary>
        /// <param name="function">Function to apply.</param>
        /// <param name="verify">Function to check each arg for validity.</param>
        /// <returns>Delegate for evaluating an expression.</returns>
        public static EvaluateExpressionDelegate Apply(Func<IReadOnlyList<dynamic>, object> function, Func<object, Expression, string> verify = null)
            =>
            (expression, state) =>
            {
                object value = null;
                string error = null;
                var args = new List<dynamic>();
                foreach (var child in expression.Children)
                {
                    (value, error) = child.TryEvaluate(state);
                    if (error != null)
                    {
                        break;
                    }
                    if (verify != null)
                    {
                        error = verify(value, child);
                    }
                    if (error != null)
                    {
                        break;
                    }
                    args.Add(value);
                }
                if (error == null)
                {
                    value = function(args);
                }
                return (value, error);
            };


        /// <summary>
        /// Generate an expression delegate that applies function on the accumulated value after verifying all children.
        /// </summary>
        /// <param name="function">Function to apply.</param>
        /// <param name="verify">Function to check each arg for validity.</param>
        /// <returns>Delegate for evaluating an expression.</returns>
        public static EvaluateExpressionDelegate ApplySequence(Func<IReadOnlyList<dynamic>, object> function, Func<object, Expression, string> verify = null)
            => Apply(
                (args) =>
                {
                    var binaryArgs = new List<object> { null, null };
                    var soFar = args[0];
                    for (var i = 1; i < args.Count; ++i)
                    {
                        binaryArgs[0] = soFar;
                        binaryArgs[1] = args[i];
                        soFar = function(binaryArgs);
                    }
                    return soFar;
                }, verify);

        /// <summary>
        /// Numeric operators that can have 1 or more args.
        /// </summary>
        /// <param name="function">Function to apply.</param>
        /// <returns>Delegate for evaluating an expression.</returns>
        public static ExpressionEvaluator Numeric(Func<IReadOnlyList<dynamic>, object> function)
            => new ExpressionEvaluator(ApplySequence(function, VerifyNumber), ExpressionReturnType.Number, ValidateNumber);

        /// <summary>
        /// Comparison operators that have 2 args and work over strings or numbers.
        /// </summary>
        /// <param name="function">Function to apply.</param>
        /// <returns>Delegate for evaluating an expression.</returns>        
        public static ExpressionEvaluator Comparison(Func<IReadOnlyList<dynamic>, object> function)
            => new ExpressionEvaluator(Apply(function, VerifyNumberOrString), ExpressionReturnType.Boolean, ValidateBinaryNumberOrString);

        /// <summary>
        /// Lookup a built-in function information by type.
        /// </summary>
        /// <param name="type">Type to look up.</param>
        /// <returns>Information about expression type.</returns>
        public static ExpressionEvaluator Lookup(string type)
        {
            if (!_functions.TryGetValue(type, out ExpressionEvaluator eval))
            {
                throw new ArgumentException($"{type} does not have a built-in expression evaluator.");
            }
            return eval;
        }

        private static void ValidateAccessor(Expression expression)
        {
            var children = expression.Children;
            if (children.Length == 0
                || !(children[0] is Constant cnst)
                || cnst.ReturnType != ExpressionReturnType.String)
            {
                throw new Exception($"{expression} must have a string as first argument.");
            }
            if (children.Length > 2)
            {
                throw new Exception($"{expression} has more than 2 children.");
            }
            if (children.Length == 2 && children[1].ReturnType != ExpressionReturnType.Object)
            {
                throw new Exception($"{expression} must have an object as its second argument.");
            }
        }

        private static (object value, string error) Accessor(Expression expression, object state)
        {
            object value = null;
            string error = null;
            object instance = state;
            var children = expression.Children;
            if (children.Length == 2)
            {
                (instance, error) = children[1].TryEvaluate(state);
            }
            else
            {
                instance = state;
            }
            if (error == null && children[0] is Constant cnst && cnst.ReturnType == ExpressionReturnType.String)
            {
                (value, error) = instance.AccessProperty((string)cnst.Value, expression);
            }
            return (value, error);
        }

        private static (object value, string error) ExtractElement(Expression expression, object state)
        {
            object value = null;
            string error = null;
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
                        int count = -1;
                        if (inst is Array arr)
                        {
                            count = arr.Length;
                        }
                        else if (inst is ICollection collection)
                        {
                            count = collection.Count;
                        }
                        var itype = inst.GetType();
                        var indexer = itype.GetProperties().Except(itype.GetDefaultMembers().OfType<PropertyInfo>());
                        if (count != -1 && indexer != null)
                        {
                            if (idx >= 0 && count > idx)
                            {
                                dynamic idyn = inst;
                                value = idyn[idx];
                                if (value is JArray jarray)
                                {
                                    value = jarray.ToArray<object>();
                                }
                                else if (value is JValue jvalue)
                                {
                                    value = jvalue.Value;
                                }
                            }
                            else
                            {
                                error = $"{index}={idx} is out of range for ${instance}";
                            }
                        }
                        else
                        {
                            error = $"{instance} is not a collection.";
                        }
                    }
                    else
                    {
                        error = $"Could not coerce {index} to an int.";
                    }
                }
            }
            return (value, error);
        }

        private static (object value, string error) And(Expression expression, object state)
        {
            object result = true;
            string error = null;
            foreach (var child in expression.Children)
            {
                (result, error) = child.TryEvaluate(state);
                if (error == null)
                {
                    if (!(result is Boolean bresult))
                    {
                        error = $"{child} is not boolean";
                        break;
                    }
                    else if (!bresult)
                    {
                        // Hit a false so stop
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            return (result, error);
        }

        private static (object value, string error) Or(Expression expression, object state)
        {
            object result = true;
            string error = null;
            foreach (var child in expression.Children)
            {
                (result, error) = child.TryEvaluate(state);
                if (error == null)
                {
                    if (!(result is Boolean bresult))
                    {
                        error = $"{child} is not boolean";
                        break;
                    }
                    else if (bresult)
                    {
                        // Hit a true so stop
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            return (result, error);
        }

        private static Dictionary<string, ExpressionEvaluator> BuildFunctionLookup()
        {
            var functions = new Dictionary<string, ExpressionEvaluator>{
                // Math
                { ExpressionType.Element, new ExpressionEvaluator(ExtractElement, ExpressionReturnType.Object,
                    (expr) => ValidateOrder(expr, ExpressionReturnType.Object, ExpressionReturnType.Number)) },
                { ExpressionType.Add, Numeric((args) => args[0] + args[1]) },
                {ExpressionType.Subtract, Numeric((args) => args[0] - args[1]) },
                {ExpressionType.Multiply, Numeric((args) => args[0] * args[1]) },
                {ExpressionType.Divide,
                    new ExpressionEvaluator(ApplySequence((args) => args[0] / args[1],
                    (value, expression) => {
                        string error = VerifyNumber(value, expression);
                        if (error == null && Convert.ToDouble(value) == 0.0)
                        {
                            error = $"Cannot divide by 0 from {expression}";
                        }
                        return error;
                    }), ExpressionReturnType.Number, ValidateNumber) },
                 {ExpressionType.Min, Numeric((args) => Math.Min(args[0], args[1])) },
                 {ExpressionType.Max, Numeric((args) => Math.Max(args[0], args[1])) },

                // Comparisons
                {ExpressionType.LessThan, Comparison((args) => args[0] < args[1]) },
                {ExpressionType.LessThanOrEqual, Comparison((args) => args[0] <= args[1]) },
                {ExpressionType.Equal, Comparison((args) => args[0] == args[1]) },
                {ExpressionType.NotEqual, Comparison((args) => args[0] != args[1]) },
                {ExpressionType.GreaterThan,Comparison((args) => args[0] > args[1]) },
                {ExpressionType.GreaterThanOrEqual, Comparison((args) => args[0] >= args[1]) },

                // Logical
                {ExpressionType.And,
                    new ExpressionEvaluator((expression, state) => And(expression, state), ExpressionReturnType.Boolean, ValidateBoolean) },
                {ExpressionType.Or,
                    new ExpressionEvaluator((expression, state) => Or(expression, state), ExpressionReturnType.Boolean, ValidateBoolean) },
                {ExpressionType.Not,
                    new ExpressionEvaluator(Apply((args) => !args[0], VerifyBoolean),
                        ExpressionReturnType.Boolean, (expression) => ValidateOrder(expression, ExpressionReturnType.Boolean)) },

                // Misc
                {ExpressionType.Accessor,
                    new ExpressionEvaluator(Accessor, ExpressionReturnType.Object, ValidateAccessor) }
            };

            // Add aliases from WDL https://docs.microsoft.com/en-us/azure/logic-apps/workflow-definition-language-functions-reference

            // Math if not already found
            // TODO: mod, rand, range 
            functions.Add("add", functions[ExpressionType.Add]);
            functions.Add("div", functions[ExpressionType.Divide]);
            functions.Add("mul", functions[ExpressionType.Multiply]);
            functions.Add("sub", functions[ExpressionType.Subtract]);

            // Comparisons
            // TODO: if
            functions.Add("and", functions[ExpressionType.And]);
            functions.Add("equals", functions[ExpressionType.Equal]);
            functions.Add("greater", functions[ExpressionType.GreaterThan]);
            functions.Add("greaterOrEquals", functions[ExpressionType.GreaterThanOrEqual]);
            functions.Add("less", functions[ExpressionType.LessThan]);
            functions.Add("lessOrEquals", functions[ExpressionType.LessThanOrEqual]);
            functions.Add("not", functions[ExpressionType.Not]);
            functions.Add("or", functions[ExpressionType.Or]);

            // TODO: Conversion functions
            // TODO: Date and time functions
            // TODO: Collection functions
            // TODO: String functions (Keep + overload for concat?)
            // TODO: Workflow functions???
            return functions;
        }

        /// <summary>
        /// Dictionary of built-in functions.
        /// </summary>
        public static Dictionary<string, ExpressionEvaluator> _functions = BuildFunctionLookup();
    }
}
