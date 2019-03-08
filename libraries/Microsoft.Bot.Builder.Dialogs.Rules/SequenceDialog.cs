using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Rules.Rules;

namespace Microsoft.Bot.Builder.Dialogs.Rules
{
    public class SequenceDialog : RuleDialog
    {
        public override List<IDialog> Steps
        {
            get
            {
                return base.Steps;
            }
            set
            {
                // Add the rules and allow the steps to work declaratively
                AddRule(new[] { new FallbackRule(value) });
            }
        }

        public SequenceDialog(string dialogId = null, List<IDialog> steps = null)
            : base(dialogId)
        {          
        }
    }
}
