using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Rules
{
    public class IntentRule : UtteranceRecognizeRule
    {
        public IntentRule(string intent = null, List<string> entities = null, List<IDialog> steps = null, string constraint = null)
            : base(intent: intent, entities: entities, steps: steps, constraint: constraint)
        {
        }
    }
}
