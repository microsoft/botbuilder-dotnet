// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using AdaptiveExpressions.Memory;
using Newtonsoft.Json.Linq;
using static AdaptiveExpressions.ExpressionFunctions;

namespace AdaptiveExpressions
{
    /// <summary>
    /// Delegate for doing static validation on an expression.
    /// </summary>
    /// <remarks>
    /// Validators can and should throw exceptions if the expression is not valid.
    /// </remarks>
    /// <param name="expression">Expression to check.</param>
    public delegate void ValidateExpressionDelegate(Expression expression);

    /// <summary>
    /// Delegate to evaluate an expression.
    /// </summary>
    /// <remarks>
    /// Evaluators should verify runtime arguments when appropriate and return an error rather than throw exceptions if possible.
    /// </remarks>
    /// <param name="expression">Expression to evaluate.</param>
    /// <param name="state">Global state information.</param>
    /// <param name="options">Options for the evaluation.</param>
    /// <returns>Value and error string that is non-null if there is an error.</returns>
    public delegate (object value, string error) EvaluateExpressionDelegate(Expression expression, IMemory state, Options options);

    /// <summary>
    /// Delegate to lookup function information from the type.
    /// </summary>
    /// <param name="type">Name to lookup, usually from <see cref="ExpressionType"/>.</param>
    /// <returns>Expression evaluation information.</returns>
    public delegate ExpressionEvaluator EvaluatorLookup(string type);

    /// <summary>
    /// Information on how to evaluate an expression.
    /// </summary>
    public class ExpressionEvaluator
    {
        private readonly ValidateExpressionDelegate _validator;
        private readonly EvaluateExpressionDelegate _evaluator;
        private ExpressionEvaluator _negation;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionEvaluator"/> class.
        /// </summary>
        /// <param name="type">Expression type.</param>
        /// <param name="evaluator">Delegate to evaluate an expression.</param>
        /// <param name="returnType">Type expected from evaluation.</param>
        /// <param name="validator">Static validation of expression.</param>
        public ExpressionEvaluator(
            string type,
            EvaluateExpressionDelegate evaluator,
            ReturnType returnType = ReturnType.Object,
            ValidateExpressionDelegate validator = null)
        {
            Type = type;
            _evaluator = evaluator;
            ReturnType = returnType;
            _validator = validator ?? new ValidateExpressionDelegate((expr) => { });
        }

        /// <summary>
        /// Gets the expression type for evaluator.
        /// </summary>
        /// <value>
        /// The type of expression from <see cref="ExpressionType"/> for built-in functions or else a unique string for custom functions.
        /// </value>
        public string Type { get; }

        /// <summary>
        /// Gets or sets the expression return type.
        /// </summary>
        /// <value>
        /// Type expected by evaluating the expression.
        /// </value>
        public ReturnType ReturnType { get; set; }

        /// <summary>
        /// Gets or sets the evaluator that is a negation of this one.
        /// </summary>
        /// <remarks>
        /// When doing <see cref="Extensions.PushDownNot(Expression)"/> then negations will replace an expression and remove not parent.
        /// By default no negation is defined and not parent will remain.
        /// If a negation is defined then this is automatically set as its negation.
        /// If an evaluator is its own negation, then the negation will be passed through to children.
        /// </remarks>
        /// <value>
        /// The evaluator that is a negation of this one.
        /// </value>
        public ExpressionEvaluator Negation
        {
            get => _negation;
            set
            {
                value._negation = this;
                _negation = value;
            }
        }

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
                    catch (Exception e)
                    {
                        error = e.Message;
                    }
                }

                value = ResolveValue(value);

                return (value, error);
            };

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

        public override string ToString() => $"{Type} => {ReturnType}";

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">Expression to evaluate.</param>
        /// <param name="state">Global state information.</param>
        /// <param name="options">Options used in the evaluation. </param>
        /// <returns>Value and error string that is non-null if there is an error.</returns>
        public (object value, string error) TryEvaluate(Expression expression, IMemory state, Options options)
            => _evaluator(expression, state, options);

        /// <summary>
        /// Validate an expression.
        /// </summary>
        /// <param name="expression">Expression to validate.</param>
        public void ValidateExpression(Expression expression)
            => _validator(expression);

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
