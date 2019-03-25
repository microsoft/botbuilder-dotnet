using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Expressions
{

    public interface IExpressionEvaluator
    {
        ExpressionReturnType ReturnType { get; }

        void ValidateExpression(Expression expression);

        (object value, string error) TryEvaluate(Expression expression, object state);
    }
}
