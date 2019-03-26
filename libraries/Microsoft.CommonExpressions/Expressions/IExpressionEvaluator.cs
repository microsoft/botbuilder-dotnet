namespace Microsoft.Expressions
{
    // TODO: Do we need this level of indirection or just use ExpressionEvaluator?
    public interface IExpressionEvaluator
    {
        ExpressionReturnType ReturnType { get; }

        void ValidateExpression(Expression expression);

        (object value, string error) TryEvaluate(Expression expression, object state);
    }
}
