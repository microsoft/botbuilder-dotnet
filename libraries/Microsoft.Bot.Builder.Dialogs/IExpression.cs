using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Expressions;

namespace Microsoft.Bot.Builder.Dialogs
{
    public interface IExpression
    {
        string Expression { get; }

        Expression Parse { get; }
    }
}
