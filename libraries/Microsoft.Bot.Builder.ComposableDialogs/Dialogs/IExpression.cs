using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.ComposableDialogs
{
    public interface IExpression
    {
        Task<object> Execute(ITurnContext turnContext);
    }

}
