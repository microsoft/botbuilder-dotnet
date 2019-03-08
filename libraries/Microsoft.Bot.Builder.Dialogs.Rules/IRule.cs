using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Dialogs.Rules
{
    public interface IRule
    {
        List<IDialog> Steps { get; }

        Task<List<PlanChangeList>> EvaluateAsync(PlanningContext planning, DialogEvent dialogEvent);
    }
}
