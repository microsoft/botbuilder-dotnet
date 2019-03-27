// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;

namespace Microsoft.Bot.Builder.Expressions
{
    public class Constant : Expression
    {
        public Constant(object value)
            : base(ExpressionType.Constant,
                  new ExpressionEvaluator((expression, state) => ((expression as Constant).Value, null),
                      (value is string ? ExpressionReturnType.String
                      : value.IsNumber() ? ExpressionReturnType.Number
                      : value is Boolean ? ExpressionReturnType.Boolean
                      : ExpressionReturnType.Object),
                      (expression) => { }))
        {
            Value = value;
        }

        public object Value { get; }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            if (Value is string str)
            {
                return $"'{Value}'";
            }
            else
            {
                return Value.ToString();
            }
        }

        public static Constant MakeExpression(object value)
            => new Constant(value);
    }
}
