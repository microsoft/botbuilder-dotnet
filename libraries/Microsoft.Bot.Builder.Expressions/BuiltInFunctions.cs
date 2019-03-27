// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Microsoft.Bot.Builder.Expressions
{
    public static class BuiltInFunctions
    {
        // Validators
        public static void ValidateArityAndType(Expression expression, int arity, params ExpressionReturnType[] types)
        {
            if (!(expression is ExpressionWithChildren tree) || (arity != -1 && tree.Children.Count != arity))
            {
                throw new ExpressionException($"Expected {arity} children", expression);
            }
            foreach (var child in tree.Children)
            {
                if (child.ReturnType != ExpressionReturnType.Object && !types.Contains(child.ReturnType))
                {
                    if (types.Count() == 1)
                    {
                        throw new ExpressionException($"Is not a {types[0]} expression", child);
                    }
                    else
                    {
                        var builder = new StringBuilder();
                        builder.Append("Is not any of [");
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
                        throw new ExpressionException(builder.ToString(), expression);
                    }
                }
            }
        }

        public static void ValidateOrder(Expression expression, params ExpressionReturnType[] types)
        {
            if (!(expression is ExpressionWithChildren tree) || tree.Children.Count != types.Count())
            {
                throw new ExpressionException($"Expected {types.Count()} children", expression);
            }
            for (var i = 0; i < tree.Children.Count; ++i)
            {
                var child = tree.Children[i];
                var type = types[i];
                if (child.ReturnType != type)
                {
                    throw new ExpressionException($"Is not a {type}", child);
                }
            }
        }

        public static void ValidateBinaryNumber(Expression expression)
            => ValidateArityAndType(expression, 2, ExpressionReturnType.Number);

        public static void ValidateBinaryNumberOrString(Expression expression)
            => ValidateArityAndType(expression, 2, ExpressionReturnType.Number, ExpressionReturnType.String);

        public static void ValidateUnary(Expression expression)
            => ValidateArityAndType(expression, 1, ExpressionReturnType.Object);

        public static void ValidateBoolean(Expression expression)
            => ValidateArityAndType(expression, -1, ExpressionReturnType.Boolean);

        // Verifiers

        public static string NoVerify(object value, Expression expression)
        {
            return null;
        }

        public static string VerifyNumber(object value, Expression expression)
        {
            string error = null;
            if (!value.IsNumber())
            {
                error = $"{expression} is not a number.";
            }
            return error;
        }

        public static string VerifyNumberOrString(object value, Expression expression)
        {
            string error = null;
            if (!value.IsNumber() && !(value is string))
            {
                error = $"{expression} is not string or number.";
            }
            return error;
        }

        public static string VerifyBoolean(object value, Expression expression)
        {
            string error = null;
            if (!(value is Boolean))
            {
                error = $"{expression} is not a boolean.";
            }
            return error;
        }

        // Apply
        public static (object value, string error) Apply(
            Func<IReadOnlyList<dynamic>, object> function,
            Expression expression,
            object state,
            Func<object, Expression, string> verify = null)
        {
            object value = null;
            string error = null;
            var args = new List<dynamic>();
            if (expression is ExpressionWithChildren tree)
            {
                foreach (var child in tree.Children)
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
            }
            if (error == null)
            {
                value = function(args);
            }
            return (value, error);
        }

        public static ExpressionEvaluator Lookup(string type)
        {
            if (!_functions.TryGetValue(type, out ExpressionEvaluator eval))
            {
                throw new ExpressionException($"{type} does not have a built-in evaluator.");
            }
            return eval;
        }

        private static (object value, string error) ExtractElement(Expression expression, object state)
        {
            var tree = expression as ExpressionWithChildren;
            var instance = tree.Children[0];
            var index = tree.Children[1];
            object value;
            string error = null;
            (value, error) = instance.TryEvaluate(state);
            if (error == null)
            {
                var inst = value;
                (value, error) = index.TryEvaluate(state);
                if (error == null)
                {
                    if (value is int idx)
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
            var tree = expression as ExpressionWithChildren;
            foreach (var child in tree.Children)
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
            var tree = expression as ExpressionWithChildren;
            foreach (var child in tree.Children)
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
                    (expr) => ValidateOrder(expr, ExpressionReturnType.Object, ExpressionReturnType.Number)) }
                , { ExpressionType.Add,
                    new ExpressionEvaluator((expression, state) => Apply((args) => args[0] + args[1], expression, state, VerifyNumberOrString),
                        ExpressionReturnType.Object, ValidateBinaryNumberOrString) }
                , {ExpressionType.Subtract,
                    new ExpressionEvaluator((expression, state) => Apply((args) => args[0] - args[1], expression, state, VerifyNumber),
                        ExpressionReturnType.Number, ValidateBinaryNumber) }
                , {ExpressionType.Multiply,
                    new ExpressionEvaluator((expression, state) => Apply((args) => args[0] * args[1], expression, state, VerifyNumber),
                        ExpressionReturnType.Number, ValidateBinaryNumber) }
                , {ExpressionType.Divide,
                    // TODO: Check for 0
                    new ExpressionEvaluator((expression, state) => Apply((args) => args[0] / args[1], expression, state, VerifyNumber),
                        ExpressionReturnType.Number, ValidateBinaryNumber) }
                , {ExpressionType.Min,
                    new ExpressionEvaluator((expression, state) => Apply((args) => Math.Min(args[0], args[1]), expression, state, VerifyNumber),
                        ExpressionReturnType.Number, ValidateBinaryNumber) }
                , {ExpressionType.Max,
                    new ExpressionEvaluator((expression, state) => Apply((args) => Math.Max(args[0], args[1]), expression, state, VerifyNumber),
                        ExpressionReturnType.Number, ValidateBinaryNumber) }

                // Comparisons
                , {ExpressionType.LessThan,
                    new ExpressionEvaluator((expression, state) => Apply((args) => args[0] < args[1], expression, state, NoVerify),
                        ExpressionReturnType.Boolean, ValidateBinaryNumberOrString) }
                , {ExpressionType.LessThanOrEqual,
                    new ExpressionEvaluator((expression, state) => Apply((args) => args[0] <= args[1], expression, state, NoVerify),
                        ExpressionReturnType.Boolean, ValidateBinaryNumberOrString) }
                , {ExpressionType.Equal,
                    new ExpressionEvaluator((expression, state) => Apply((args) => args[0] == args[1], expression, state, NoVerify),
                        ExpressionReturnType.Boolean, ValidateBinaryNumberOrString) }
                , {ExpressionType.NotEqual,
                     new ExpressionEvaluator((expression, state) => Apply((args) => args[0] != args[1], expression, state, NoVerify),
                        ExpressionReturnType.Boolean, ValidateBinaryNumberOrString) }
                , {ExpressionType.GreaterThan,
                    new ExpressionEvaluator((expression, state) => Apply((args) => args[0] > args[1], expression, state, NoVerify),
                        ExpressionReturnType.Boolean, ValidateBinaryNumberOrString) }
                , {ExpressionType.GreaterThanOrEqual,
                    new ExpressionEvaluator((expression, state) => Apply((args) => args[0] >= args[1], expression, state, NoVerify),
                        ExpressionReturnType.Boolean, ValidateBinaryNumberOrString) }

                // Logical
                , {ExpressionType.And,
                    new ExpressionEvaluator((expression, state) => And(expression, state),
                        ExpressionReturnType.Boolean, ValidateBoolean) }
                , {ExpressionType.Or,
                    new ExpressionEvaluator((expression, state) => Or(expression, state),
                        ExpressionReturnType.Boolean, ValidateBoolean) }
                , {ExpressionType.Not,
                    new ExpressionEvaluator((expression, state) => Apply((args) => !args[0], expression, state, VerifyBoolean),
                        ExpressionReturnType.Boolean, (expression) => ValidateOrder(expression, ExpressionReturnType.Boolean)) }
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

        public static Dictionary<string, ExpressionEvaluator> _functions = BuildFunctionLookup();
    }
}
