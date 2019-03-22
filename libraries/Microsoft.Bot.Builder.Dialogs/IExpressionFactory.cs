using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Dialogs.Expressions;

namespace Microsoft.Bot.Builder.Dialogs
{
    public interface IExpressionFactory
    {
        IExpression CreateExpression(string expression);
    }
}
