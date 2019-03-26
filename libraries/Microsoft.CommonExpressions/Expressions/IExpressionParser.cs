namespace Microsoft.Expressions
{
    public interface IExpressionParser
    {
        Expression Parse(string expression);
    }
}
