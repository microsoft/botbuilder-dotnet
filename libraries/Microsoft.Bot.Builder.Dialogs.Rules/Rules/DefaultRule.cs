using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Rules
{
    public class DefaultRule : EventRule
    {
        public DefaultRule(List<IDialog> steps = null, PlanChangeTypes changeType = PlanChangeTypes.DoSteps)
            : base(new List<string>() { PlanningEvents.Fallback.ToString() }, steps, changeType)
        {
        }
    }
}
