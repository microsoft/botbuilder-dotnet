// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Bot.Builder.Expressions
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
        public Expression(string type, ExpressionEvaluator evaluator = null, params Expression[] children)
        {
            Type = type;
            _evaluator = evaluator ?? BuiltInFunctions.Lookup(type);
            Children = children;
        }

        public string Type { get; }

        public Expression[] Children { get; set;}

        public ExpressionReturnType ReturnType { get { return _evaluator.ReturnType; } }

        protected ExpressionEvaluator _evaluator { get; }

        public void Validate()
        {
            _evaluator.ValidateExpression(this);
        }

        public void ValidateAll()
        {
            Validate();
            foreach(var child in Children)
            {
                child.ValidateAll();
            }
        }

        public (object value, string error) TryEvaluate(object state)
            => _evaluator.TryEvaluate(this, state);

        public override string ToString()
        {
            return ToString(Type);
        }

        protected string ToString(string name)
        {
            var builder = new StringBuilder();
            builder.Append(Type);
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
                    builder.Append(", ");
                }

                builder.Append(child.ToString());
            }
            builder.Append(')');
            return builder.ToString();
        }

        public static Expression MakeExpression(string type, ExpressionEvaluator evaluator = null, params Expression[] children)
        {
            var expr = new Expression(type, evaluator, children);
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
            => Expression.MakeExpression(ExpressionType.And, null, children);

        public static Expression OrExpression(params Expression[] children)
            => Expression.MakeExpression(ExpressionType.Or, null, children);

        public static Expression NotExpression(Expression child)
            => Expression.MakeExpression(ExpressionType.Not, null, child);

        public static Expression ConstantExpression(object value)
            => Constant.MakeExpression(value);

        public static Expression Accessor(string property, Expression instance = null)
            => instance == null
            ? MakeExpression(ExpressionType.Accessor, null, ConstantExpression(property))
            : MakeExpression(ExpressionType.Accessor, null, ConstantExpression(property), instance);
    }
}
