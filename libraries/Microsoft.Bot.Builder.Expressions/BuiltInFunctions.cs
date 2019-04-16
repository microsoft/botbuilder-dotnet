// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Expressions
{
    /// <summary>
    /// Definition of default built-in functions for expressions.
    /// </summary>
    /// <remarks>
    /// These functions are largely from WDL https://docs.microsoft.com/en-us/azure/logic-apps/workflow-definition-language-functions-reference
    /// with a few extensions like infix operators for math, logic and comparisons.
    /// </remarks>
    public static class BuiltInFunctions
    {
        /// <summary>
        /// Random number generator used for expressions.
        /// </summary>
        public static Random Randomizer = new Random();

        /// <summary>
        /// The default date time format string.
        /// </summary>
        public static readonly string DefaultDateTimeFormat = "o";

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
            => ValidateArityAndAnyType(expression, 1, int.MaxValue, ReturnType.Number);

        /// <summary>
        /// Validate 1 or more boolean arguments.
        /// </summary>
        /// <param name="expression">Expression to validate.</param>
        public static void ValidateBoolean(Expression expression)
            => ValidateArityAndAnyType(expression, 1, int.MaxValue, ReturnType.Boolean);

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
        /// Verify value is an integer.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="expression">Expression that led to value.</param>
        /// <returns>Error or null if valid.</returns>
        public static string VerifyInteger(object value, Expression expression)
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
        /// <returns>Error or null if valid.</returns>
        public static string VerifyString(object value, Expression expression)
        {
            string error = null;
            if (!(value is string))
            {
                error = $"{expression} is not a string.";
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
            if (!(value is bool))
            {
                error = $"{expression} is not a boolean.";
            }
            return error;
        }

        // Apply -- these are helpers for adding functions to the expression library.

        /// <summary>
        /// Evaluate expression children and return them.
        /// </summary>
        /// <param name="expression">Expression with children.</param>
        /// <param name="state">Global state.</param>
        /// <param name="verify">Optional function to verify each child's result.</param>
        /// <returns>List of child values or error message.</returns>
        public static (IReadOnlyList<dynamic>, string error) EvaluateChildren(Expression expression, object state,
            Func<object, Expression, string> verify = null)
        {
            var args = new List<dynamic>();
            object value;
            string error = null;
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
            return (args, error);
        }

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
                IReadOnlyList<dynamic> args;
                (args, error) = EvaluateChildren(expression, state, verify);
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
                args =>
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
            => new ExpressionEvaluator(ApplySequence(function, VerifyNumber), ReturnType.Number, ValidateNumber);

        /// <summary>
        /// Comparison operators that have 2 args and work over strings or numbers.
        /// </summary>
        /// <param name="function">Function to apply.</param>
        /// <returns>Delegate for evaluating an expression.</returns>        
        public static ExpressionEvaluator Comparison(Func<IReadOnlyList<dynamic>, object> function)
            => new ExpressionEvaluator(Apply(function, VerifyNumberOrString), ReturnType.Boolean, ValidateBinaryNumberOrString);

        /// <summary>
        /// Transform a string into another string.
        /// </summary>
        /// <param name="function">Function to apply.</param>
        /// <returns>Delegate for evaluating an expression.</returns>
        public static ExpressionEvaluator StringTransform(Func<IReadOnlyList<dynamic>, object> function)
            => new ExpressionEvaluator(Apply(function, VerifyString), ReturnType.String, ValidateUnaryString);

        /// <summary>
        /// Transform a datetime to another datetime.
        /// </summary>
        /// <param name="function">Transformer.</param>
        /// <returns>Delegate for evaluating expression.</returns>
        public static ExpressionEvaluator TimeTransform(Func<DateTime, int, DateTime> function)
            => new ExpressionEvaluator((expr, state) =>
                {
                    object value = null;
                    string error = null;
                    IReadOnlyList<dynamic> args;
                    (args, error) = EvaluateChildren(expr, state);
                    if (error == null)
                    {
                        if (args[0] is string string0 && args[1] is int int1)
                        {
                            var formatString = (args.Count() == 3 && args[2] is string string1) ? string1 : DefaultDateTimeFormat;
                            var timestamp = ParseTimestamp(string0);
                            value = function(timestamp, int1).ToString(formatString);
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
        /// Lookup a built-in function information by type.
        /// </summary>
        /// <param name="type">Type to look up.</param>
        /// <returns>Information about expression type.</returns>
        public static ExpressionEvaluator Lookup(string type)
        {
            if (!_functions.TryGetValue(type, out var eval))
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

        private static (object value, string error) Accessor(Expression expression, object state)
        {
            object value = null;
            string error = null;
            var instance = state;
            var children = expression.Children;
            if (children.Length == 2)
            {
                (instance, error) = children[1].TryEvaluate(state);
            }
            else
            {
                instance = state;
            }
            if (error == null && children[0] is Constant cnst && cnst.ReturnType == ReturnType.String)
            {
                (value, error) = AccessProperty(instance, (string)cnst.Value);
            }
            return (value, error);
        }

        private static (object value, string error) Property(Expression expression, object state)
        {
            object value = null;
            string error = null;
            object instance = null;
            object property = null;

            var children = expression.Children;
            (instance, error) = children[0].TryEvaluate(state);
            (property, error) = children[1].TryEvaluate(state);
            if(error == null)
            {
                (value, error) =  AccessProperty(instance, (string)property);
            }

            return (value, error);
            
        }
        /// <summary>
        /// Lookup a property in IDictionary, JObject or through reflection.
        /// </summary>
        /// <param name="instance">Instance with property.</param>
        /// <param name="property">Property to lookup.</param>
        /// <param name="expression">Expression that generated instance.</param>
        /// <returns>Value and error information if any.</returns>
        private static (object value, string error) AccessProperty(object instance, string property)
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
                            value = GetSpecificTypeFromJValue(jvalue);
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
                        var count = -1;
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
                                    value = GetSpecificTypeFromJValue(jvalue);
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
                    else if(idxValue is string idxStr)
                    {
                        (value, error) = AccessProperty(inst, idxStr);
                    }
                    else
                    {
                        error = $"Could not coerce {index} to an int or string";
                    }
                }
            }
            return (value, error);
        }

        private static object GetSpecificTypeFromJValue(JValue jvalue)
        {
            var value = jvalue.Value;
            if (jvalue.Type == JTokenType.Integer)
            {
                value = jvalue.ToObject<int>();
            }
            else if (jvalue.Type == JTokenType.String)
            {
                value = jvalue.ToObject<string>();
            }
            else if (jvalue.Type == JTokenType.Boolean)
            {
                value = jvalue.ToObject<bool>();
            }
            else if (jvalue.Type == JTokenType.Float)
            {
                value = jvalue.ToObject<double>();
            }
            return value;
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
                    if (!(result is bool bresult))
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
                    if (!(result is bool bresult))
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

        


        private static (object value, string error) Substring(Expression expression, object state)
        {
            object result = null;
            string error = null;
            dynamic str;
            dynamic start;
            dynamic length;
            (str, error) = expression.Children[0].TryEvaluate(state);
            if (expression.Children.Length == 2)
            {
                // Support just have start index
                length = str.Length;
            }

            if (error == null)
            {
                var startExpr = expression.Children[1];
                (start, error) = startExpr.TryEvaluate(state);
                if (error == null && !(start is int))
                {
                    error = $"{startExpr} is not an integer.";
                }
                else if (start < 0 || start >= str.Length)
                {
                    error = $"{startExpr}={start} which is out of range for {str}.";
                }
                if (error == null)
                {
                    var lengthExpr = expression.Children[2];
                    (length, error) = lengthExpr.TryEvaluate(state);
                    if (error == null && !(length is int))
                    {
                        error = $"{lengthExpr} is not an integer.";
                    }
                    else if (length < 0 || start + length > str.Length)
                    {
                        error = $"{lengthExpr}={length} which is out of range for {str}.";
                    }
                    if (error == null)
                    {
                        result = str.Substring(start, length);
                    }
                }
            }
            return (result, error);
        }

        private static (object value, string error) Foreach(Expression expression, object state)
        {
            object result = null;
            string error = null;

            dynamic collection;
            (collection, error) = expression.Children[0].TryEvaluate(state);
            if (error == null)
            {
                // 2nd parameter has been rewrite to $local.item
                var iteratorName = (string)(expression.Children[1].Children[0] as Constant).Value;
                
                if (collection is IList ilist)
                {
                    result = new List<object>();
                    for (int idx = 0; idx < ilist.Count; idx++)
                    {
                        var local = new Dictionary<string, object>
                        {
                            {iteratorName, ilist[idx]},
                        };
                        var newScope = new Dictionary<string, object>
                        {
                            {"$global", state},
                            {"$local", local}
                        };

                        (var r, var e) = expression.Children[2].TryEvaluate(newScope);
                        if (e != null)
                        {
                            return (null, e);
                        }

                        ((List<object>)result).Add(r);
                    }
                }
                
                else
                {
                    error = $"{expression.Children[0]} is not a collection to run foreach";
                }
            }
            return (result, error);
        }


        //
        private static void ValidateForeach(Expression expression)
        {
            if (expression.Children.Count() != 3)
            {
                throw new Exception($"foreach expect 3 parameters, acutal {expression.Children.Count()}");
            }

            var second = expression.Children[1];

            if (!(second.Type == ExpressionType.Accessor && second.Children.Count() == 1))
            {
                throw new Exception($"Second paramter of foreach is not an identifier : {second}");
            }

            var iteratorName = second.ToString();

            // rewrite the 2nd, 3rd paramater
            expression.Children[1] = RewriteAccessor(expression.Children[1], iteratorName);
            expression.Children[2] = RewriteAccessor(expression.Children[2], iteratorName);

        }

        private static Expression RewriteAccessor(Expression expression, string localVarName)
        {
            if (expression.Type == ExpressionType.Accessor)
            {
                if (expression.Children.Count() == 2)
                {
                    expression.Children[1] = RewriteAccessor(expression.Children[1], localVarName);
                }
                else
                {
                    var str = expression.ToString();
                    var prefix = "$global";
                    if (str == localVarName || str.StartsWith(localVarName + "."))
                    {
                        prefix = "$local";
                    }

                    expression.Children = new Expression[] { expression.Children[0],
                                                  Expression.MakeExpression(ExpressionType.Accessor, null, new Constant(prefix)) };

                }

                return expression;
            }
            else
            {
                // rewite children if have any
                for (int idx = 0; idx < expression.Children.Count(); idx++)
                {
                    expression.Children[idx] = RewriteAccessor(expression.Children[idx], localVarName);
                }
                return expression;
            }
        }

        private static TimeSpan GetTimeSpan(long interval, string timeUnit)
        {
            switch (timeUnit)
            {
                //TODO support month and year
                case "Second":
                    return TimeSpan.FromSeconds(interval);
                case "Minute":
                    return TimeSpan.FromMinutes(interval);
                case "Hour":
                    return TimeSpan.FromHours(interval);
                case "Day":
                    return TimeSpan.FromDays(interval);
                case "Week":
                    return TimeSpan.FromDays(interval * 7);
            }
            // TODO: Should convert this to pass errors
            throw new ArgumentException($"{timeUnit} is not a valid time unit.");
        }

        private static DateTime ParseTimestamp(string timeStamp)
        {
            if (!DateTime.TryParse(
              s: timeStamp,
              provider: CultureInfo.InvariantCulture,
              styles: DateTimeStyles.RoundtripKind,
              result: out var parsedTimestamp))
            {
                throw new Exception();
            }

            return parsedTimestamp;
        }

        private static bool IsSameDay(DateTime date1, DateTime date2) => date1.Year == date2.Year && date1.Month == date2.Month && date1.Day == date2.Day;



        private static Dictionary<string, ExpressionEvaluator> BuildFunctionLookup()
        {
            var functions = new Dictionary<string, ExpressionEvaluator>
            {
                // Math
                { ExpressionType.Element, new ExpressionEvaluator(ExtractElement, ReturnType.Object,ValidateBinary) },
                { ExpressionType.Add, Numeric(args => args[0] + args[1]) },
                { ExpressionType.Subtract, Numeric(args => args[0] - args[1]) },
                { ExpressionType.Multiply, Numeric(args => args[0] * args[1]) },
                { ExpressionType.Divide,
                    new ExpressionEvaluator(ApplySequence(args => args[0] / args[1],
                    (value, expression) => {
                        var error = VerifyNumber(value, expression);
                        if (error == null && Convert.ToDouble(value) == 0.0)
                        {
                            error = $"Cannot divide by 0 from {expression}";
                        }
                        return error;
                    }), ReturnType.Number, ValidateNumber) },
                { ExpressionType.Min, Numeric(args => Math.Min(args[0], args[1])) },
                { ExpressionType.Max, Numeric(args => Math.Max(args[0], args[1])) },
                { ExpressionType.Power, Numeric(args => Math.Pow(args[0], args[1])) },
                { ExpressionType.Mod,
                    new ExpressionEvaluator(Apply(args => args[0] % args[1], VerifyInteger),
                        ReturnType.Number, ValidateBinaryNumber) },
                { ExpressionType.Average,
                    new ExpressionEvaluator(Apply(args => ((IList<object>)args[0]).Average(u => Convert.ToDouble(u))),
                        ReturnType.Number, ValidateUnary) },
                { ExpressionType.Sum,
                    new ExpressionEvaluator(Apply(args =>    {
                        var operands = (IList<object>)args[0];
                        if (operands.All(u => (u is int))) return operands.Sum(u => (int)u);
                        if (operands.All(u => ((u is int) || (u is double)))) return operands.Sum(u => Convert.ToDouble(u));
                        return 0;
                    }),
                        ReturnType.Number, ValidateUnary) },
                { ExpressionType.Count,
                    new ExpressionEvaluator(Apply(args => ((IList<object>)args[0]).Count), ReturnType.Number, ValidateUnary)},

                // Booleans
                { ExpressionType.LessThan, Comparison(args => args[0] < args[1]) },
                { ExpressionType.LessThanOrEqual, Comparison(args => args[0] <= args[1]) },
                { ExpressionType.Equal,
                    new ExpressionEvaluator(Apply(args => args[0] == args[1]), ReturnType.Boolean, ValidateBinary) },
                { ExpressionType.NotEqual,
                    new ExpressionEvaluator(Apply(args => args[0] != args[1]), ReturnType.Boolean, ValidateBinary) },
                { ExpressionType.GreaterThan,Comparison(args => args[0] > args[1]) },
                { ExpressionType.GreaterThanOrEqual, Comparison(args => args[0] >= args[1]) },
                { ExpressionType.Exists, new ExpressionEvaluator(Apply(args => args[0] != null), ReturnType.Boolean, ValidateUnary) },
                { ExpressionType.And,
                    new ExpressionEvaluator((expression, state) => And(expression, state), ReturnType.Boolean, ValidateBoolean) },
                { ExpressionType.Or,
                    new ExpressionEvaluator((expression, state) => Or(expression, state), ReturnType.Boolean, ValidateBoolean) },
                { ExpressionType.Not,
                    new ExpressionEvaluator(Apply(args => !args[0], VerifyBoolean), ReturnType.Boolean, ValidateUnaryBoolean) },
                { ExpressionType.Contains,
                    new ExpressionEvaluator(Apply(args =>
                    {
                        if (args[0] is string string0 && args[1] is string string1)
                        {
                            if (string0.Contains(string1))
                                return true;
                        }
                        //list to find a value
                        else if (args[0] is IList list1)
                        {
                            if (list1.Contains(args[1]))
                                return true;
                        }
                        //Dictionary contains key
                        else if (args[0] is IDictionary dict && args[1] is string string2)
                        {
                            if (dict is Dictionary<string, object> realdict
                                && realdict.ContainsKey(string2))
                                return true;
                        }
                        else if(args[1] is string string3)
                        {
                            var propInfo = args[0].GetType().GetProperty(string3);
                            if (propInfo != null)
                            {
                                return true;
                            }
                        }
                        return false;
                    }), ReturnType.Boolean, ValidateBinary) },
                { ExpressionType.Empty,
                    new ExpressionEvaluator(Apply(args => {
                           if (args[0] == null) return true;
                           if (args[0] is string string0) return string.IsNullOrEmpty(string0);
                           if (args[0] is IList list) return list.Count == 0;
                           return args[0].GetType().GetProperties().Length == 0;
                    }), ReturnType.Boolean, ValidateUnary) }, 

                // String
                { ExpressionType.Concat,
                    new ExpressionEvaluator(
                        Apply(args =>
                        {
                            var builder = new StringBuilder();
                            foreach (var arg in args)
                            {
                                builder.Append(arg);
                            }
                            return builder.ToString();
                        }, VerifyString),
                        ReturnType.String, ValidateString) },
                { ExpressionType.Length,
                    new ExpressionEvaluator(Apply(args => args[0].Length, VerifyString), ReturnType.Number, ValidateUnaryString) },
                { ExpressionType.Replace,
                    new ExpressionEvaluator(Apply(args => args[0].Replace(args[1], args[2]), VerifyString),
                        ReturnType.String,
                        (expression) => ValidateArityAndAnyType(expression, 3, 3, ReturnType.String)) },
                { ExpressionType.ReplaceIgnoreCase,
                    new ExpressionEvaluator(
                        Apply(args => Regex.Replace(args[0], args[1], args[2], RegexOptions.IgnoreCase), VerifyString),
                        ReturnType.String,
                        (expression) => ValidateArityAndAnyType(expression, 3, 3, ReturnType.String)) },
                { ExpressionType.Split,
                    new ExpressionEvaluator(
                        Apply(args => args[0].Split(args[1].ToCharArray()), VerifyString),
                        ReturnType.Object,
                        (expression) => ValidateArityAndAnyType(expression, 2, 2, ReturnType.String)) },
                { ExpressionType.Substring,
                    new ExpressionEvaluator(
                        Substring, ReturnType.String,
                        (expression) => ValidateOrder(expression, new[] { ReturnType.Number }, ReturnType.String, ReturnType.Number)) },
                { ExpressionType.ToLower, StringTransform(args => args[0].ToLower())},
                { ExpressionType.ToUpper, StringTransform(args => args[0].ToUpper())},
                { ExpressionType.Trim, StringTransform(args => args[0].Trim())},
                { ExpressionType.Join,
                    new ExpressionEvaluator(
                        Apply(args => string.Join(args[1], ((IList) args[0]).OfType<object>().Select(x => x.ToString()))),
                        ReturnType.String,
                        expr => ValidateOrder(expr, null, ReturnType.Object, ReturnType.String))},

                // Date and time
                { ExpressionType.AddDays, TimeTransform((ts, add) => ts.AddDays(add)) },
                { ExpressionType.AddHours, TimeTransform((ts, add) => ts.AddHours(add)) },
                { ExpressionType.AddMinutes, TimeTransform((ts, add) => ts.AddMinutes(add)) },
                { ExpressionType.AddSeconds, TimeTransform((ts, add) => ts.AddSeconds(add)) },
                { ExpressionType.DayOfMonth, new ExpressionEvaluator(
                   Apply(args => ParseTimestamp(args[0]).Day, VerifyString), ReturnType.Number, ValidateUnaryString) },
                { ExpressionType.DayOfWeek, new ExpressionEvaluator(
                   Apply(args => (int)ParseTimestamp(args[0]).DayOfWeek, VerifyString), ReturnType.Number, ValidateUnaryString) },
                { ExpressionType.DayOfYear, new ExpressionEvaluator(
                   Apply(args => ParseTimestamp(args[0]).DayOfYear, VerifyString), ReturnType.Number, ValidateUnaryString) },
                { ExpressionType.Month, new ExpressionEvaluator(
                   Apply(args => ParseTimestamp(args[0]).Month, VerifyString), ReturnType.Number, ValidateUnaryString) },
                { ExpressionType.Date, new ExpressionEvaluator(
                   Apply(args => ParseTimestamp(args[0]).Date.ToString("M/dd/yyyy"), VerifyString), ReturnType.String, ValidateUnaryString) },
                { ExpressionType.Year, new ExpressionEvaluator(
                   Apply(args => ParseTimestamp(args[0]).Year, VerifyString), ReturnType.Number, ValidateUnaryString) },
                { ExpressionType.UtcNow, new ExpressionEvaluator(
                   Apply(args => DateTime.UtcNow.ToString(args.Count() == 1 ? args[0] : DefaultDateTimeFormat), VerifyString),
                   ReturnType.String) },
                { ExpressionType.FormatDateTime, new ExpressionEvaluator(
                   Apply(args => ParseTimestamp(args[0]).ToString(args.Count() == 2 ? args[1] : DefaultDateTimeFormat), VerifyString),
                   ReturnType.String, (expr) => ValidateOrder(expr, new[] { ReturnType.String }, ReturnType.String)) },
                { ExpressionType.SubtractFromTime, new ExpressionEvaluator(
                   (expr, state) =>
                   {
                       object value = null;
                       string error = null;
                       IReadOnlyList<dynamic> args;
                       (args, error) = EvaluateChildren(expr, state);
                       if (error == null)
                       {
                           if (args[0] is string string0 && args[1] is int int1 && args[2] is string string2)
                           {
                               var format = (args.Count() == 4) ? (string)args[3] : DefaultDateTimeFormat;
                               value = ParseTimestamp(string0).Subtract(GetTimeSpan(int1, string2)).ToString(format);
                           }
                           else
                           {
                               error = $"{expr} can't evaluate.";
                           }
                       }
                       return (value, error);
                   },
                   ReturnType.String,
                   (expr) => ValidateOrder(expr, new[] { ReturnType.String }, ReturnType.String, ReturnType.Number, ReturnType.String)) },
                { ExpressionType.DateReadBack, new ExpressionEvaluator(
                   Apply(args =>
                   {
                       object value = null;
                       var timestamp1 = ParseTimestamp(args[0]).Date;
                       var timestamp2 = ParseTimestamp(args[1]).Date;
                       if (IsSameDay(timestamp1, timestamp2))
                       {
                           value = "Today";
                       }
                       if (IsSameDay(timestamp1.AddDays(1), timestamp2))
                       {
                           value = "Tomorrow";
                       }
                       else if (IsSameDay(timestamp1.AddDays(2), timestamp2))
                       {
                           value = "The day after tomorrow";
                       }
                       else if (IsSameDay(timestamp1.AddDays(-1), timestamp2))
                       {
                           value = "Yesterday";
                       }
                       else if (IsSameDay(timestamp1.AddDays(-2), timestamp2))
                       {
                           value = "The day before yesterday";
                       }
                       return value;
                   }, VerifyString),
                   ReturnType.String, expr => ValidateOrder(expr, null, ReturnType.String, ReturnType.String)) },
                { ExpressionType.GetTimeOfDay, new ExpressionEvaluator(
                   Apply(args =>
                   {
                       object value = null;
                       var timestamp = ParseTimestamp(args[0]);
                       if (timestamp.Hour == 0 && timestamp.Minute == 0) value = "midnight";
                       else if (timestamp.Hour >= 0 && timestamp.Hour < 12) value = "morning";
                       else if (timestamp.Hour == 12 && timestamp.Minute == 0) value = "noon";
                       else if (timestamp.Hour < 18) value = "afternoon";
                       else if (timestamp.Hour < 22 || (timestamp.Hour == 22 && timestamp.Minute == 0)) value = "evening";
                       else value = "night";
                       return value;
                   }, VerifyString),
                   ReturnType.String, expr => ValidateOrder(expr, null, ReturnType.String)) },

                // Conversions
                { ExpressionType.Float,
                    new ExpressionEvaluator(Apply(args => (float)Convert.ToDouble(args[0])), ReturnType.Number, ValidateUnary) },
                { ExpressionType.Int,
                    new ExpressionEvaluator(Apply(args => Convert.ToInt32(args[0])), ReturnType.Number, ValidateUnary) },
                // TODO: Is this really the best way?
                { ExpressionType.String,
                    new ExpressionEvaluator(Apply(args => JsonConvert.SerializeObject(args[0]).TrimStart('"').TrimEnd('"')), ReturnType.String, ValidateUnary) },
                { ExpressionType.Bool,
                    new ExpressionEvaluator(Apply(args => Convert.ToBoolean(args[0])), ReturnType.Boolean, ValidateUnary) },
            
                // Misc
                { ExpressionType.Accessor,
                    new ExpressionEvaluator(Accessor, ReturnType.Object, ValidateAccessor) },
                 { ExpressionType.Property,
                    new ExpressionEvaluator(Property, ReturnType.Object, (expr) => ValidateOrder(expr, null, ReturnType.Object, ReturnType.String)) },
                { ExpressionType.If,
                    new ExpressionEvaluator(
                        Apply(args => args[0] ? args[1] : args[2]),
                        ReturnType.Object,
                        (expr) => ValidateOrder(expr, null, ReturnType.Boolean, ReturnType.Object, ReturnType.Object)) },
                { ExpressionType.Rand,
                    new ExpressionEvaluator(Apply(args => Randomizer.Next(args[0], args[1]), VerifyInteger),
                        ReturnType.Number, ValidateBinaryNumber) },
                { ExpressionType.CreateArray,
                    new ExpressionEvaluator(Apply(args => new List<object>(args)), ReturnType.Object) },
                { ExpressionType.First, new ExpressionEvaluator(Apply(args =>
                    {
                        if (args[0] is string string0 && string0.Length > 0) return string0.First().ToString();
                        if (args[0] is IList list && list.Count > 0) return list[0];
                        return null;
                    }), ReturnType.Object, ValidateUnary) },
                { ExpressionType.Last, new ExpressionEvaluator(Apply(args =>
                    {
                        if (args[0] is string string0 && string0.Length > 0) return string0.Last().ToString();
                        if (args[0] is IList list && list.Count > 0) return list[list.Count - 1];
                        return null;
                    }), ReturnType.Object, ValidateUnary) },

                // Object manipulation and construction functions
                // TODO
                { ExpressionType.Json,
                    new ExpressionEvaluator(Apply(args => JToken.Parse(args[0])), ReturnType.String, (expr) => ValidateOrder(expr, null, ReturnType.String)) },
                { ExpressionType.AddProperty,
                    new ExpressionEvaluator(Apply(args => {var newJobj = (JObject)args[0]; newJobj[args[1].ToString()] = args[2];return newJobj; }), 
                    ReturnType.Object, (expr) => ValidateOrder(expr, null, ReturnType.Object, ReturnType.String, ReturnType.Object)) },
                { ExpressionType.SetProperty,
                   new ExpressionEvaluator(Apply(args => {var newJobj = (JObject)args[0]; newJobj[args[1].ToString()] = args[2];return newJobj; }),
                    ReturnType.Object, (expr) => ValidateOrder(expr, null, ReturnType.Object, ReturnType.String, ReturnType.Object)) },
                { ExpressionType.RemoveProperty,
                    new ExpressionEvaluator(Apply(args => {var newJobj = (JObject)args[0]; newJobj.Property(args[1].ToString()).Remove();return newJobj; }),
                    ReturnType.Object, (expr) => ValidateOrder(expr, null, ReturnType.Object, ReturnType.String)) },

                { ExpressionType.Foreach, new ExpressionEvaluator(Foreach, ReturnType.Object, ValidateForeach)},
            };

            // Math aliases
            functions.Add("add", functions[ExpressionType.Add]);
            functions.Add("div", functions[ExpressionType.Divide]);
            functions.Add("mul", functions[ExpressionType.Multiply]);
            functions.Add("sub", functions[ExpressionType.Subtract]);
            functions.Add("exp", functions[ExpressionType.Power]);
            functions.Add("mod", functions[ExpressionType.Mod]);

            // Comparison aliases
            functions.Add("and", functions[ExpressionType.And]);
            functions.Add("equals", functions[ExpressionType.Equal]);
            functions.Add("greater", functions[ExpressionType.GreaterThan]);
            functions.Add("greaterOrEquals", functions[ExpressionType.GreaterThanOrEqual]);
            functions.Add("less", functions[ExpressionType.LessThan]);
            functions.Add("lessOrEquals", functions[ExpressionType.LessThanOrEqual]);
            functions.Add("not", functions[ExpressionType.Not]);
            functions.Add("or", functions[ExpressionType.Or]);

            functions.Add("concat", functions[ExpressionType.Concat]);
            return functions;
        }

        /// <summary>
        /// Dictionary of built-in functions.
        /// </summary>
        public static Dictionary<string, ExpressionEvaluator> _functions = BuildFunctionLookup();
    }
}
