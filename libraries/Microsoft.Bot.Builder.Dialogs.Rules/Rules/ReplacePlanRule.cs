using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Rules
{
    public class ReplacePlanRule : UtteranceRecognizeRule
    {
        public ReplacePlanRule(string intent = null, List<string> entities = null, List<IDialog> steps = null, string constraint = null)
            : base(intent, entities, steps, PlanChangeTypes.ReplacePlan, constraint: constraint)
        {
        }
    }
}
