// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;

namespace Microsoft.Expressions
{
    public enum ExpressionReturnType
    {
        Boolean
        , Number
        , Object
        , String
    }

    public class Expression 
    {
        public Expression(string type, ExpressionEvaluator evaluator = null)
        {
            Type = type;
            _evaluator = evaluator ?? BuiltInFunctions.Lookup(type);
        }

        public string Type { get; }

        public ExpressionReturnType ReturnType { get { return _evaluator.ReturnType; } }

        protected ExpressionEvaluator _evaluator { get; }

        public void Validate()
        {
            _evaluator.ValidateExpression(this);
        }

        public (object value, string error) TryEvaluate(object state)
            => _evaluator.TryEvaluate(this, state);

        public virtual void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public static Expression MakeExpression(string type, ExpressionEvaluator evaluator = null)
        {
            var expr = new Expression(type, evaluator);
            expr.Validate();
            return expr;
        }

        public static Expression LambaExpression(EvaluateExpressionDelegate function)
            => new Expression(ExpressionType.Lambda, new ExpressionEvaluator(function));

        public static Expression Lambda(Func<object, object> function)
            => new Expression(ExpressionType.Lambda,
                new ExpressionEvaluator((expression, state) =>
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

        public static Expression AndExpression(params Expression[] children)
            => ExpressionWithChildren.MakeExpression(ExpressionType.And, children);

        public static Expression OrExpression(params Expression[] children)
            => ExpressionWithChildren.MakeExpression(ExpressionType.Or, children);

        public static Expression NotExpression(Expression child)
            => ExpressionWithChildren.MakeExpression(ExpressionType.Not, new Expression[] { child });

        public static Expression ConstantExpression(object value)
            => Constant.MakeExpression(value);
    }
}
