using Microsoft.Expressions;

namespace Microsoft.Bot.Builder.Dialogs.Expressions
{
    public interface IExpressionParser
    {
        Expression Parse();
    }
}
