namespace Microsoft.Expressions
{
    public interface IExpressionVisitor
    {
        void Visit(Accessor expression);
        void Visit(Constant expression);
        void Visit(Expression expression);
        void Visit(ExpressionWithChildren expression);
    }
}
