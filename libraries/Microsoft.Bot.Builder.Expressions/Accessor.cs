// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Expressions
{
    public class Accessor : ExpressionWithChildren
    {
        public Accessor(string property, Expression instance = null)
            : base(ExpressionType.Accessor, instance != null ? new List<Expression> { instance } : new List<Expression>(), _accessor)
        {
            Property = property;
        }

        private static ExpressionEvaluator _accessor = new ExpressionEvaluator(
             (expression, state) => (expression as Accessor).Evaluate(state),
             ExpressionReturnType.Object,
            (expression) => { });

        public string Property { get; }

        private (object value, string error) Evaluate(object state)
        {
            object value = null;
            string error = null;
            object instance = state;
            if (Children.Count == 1)
            {
                (instance, error) = Children[0].TryEvaluate(state);
            }
            else
            {
                instance = state;
            }
            if (error == null)
            {
                (value, error) = instance.AccessProperty(Property, this);
            }
            return (value, error);
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            var instance = Children.Count == 0 ? "<state>" : Children[0].ToString();
            return $"{instance}.{Property}";
        }

        public static Accessor MakeExpression(string property, Expression instance = null)
        {
            var expr = new Accessor(property, instance);
            expr.Validate();
            return expr;
        }
    }
}
