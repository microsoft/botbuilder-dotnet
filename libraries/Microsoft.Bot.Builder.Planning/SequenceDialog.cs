using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Planning.Rules;

namespace Microsoft.Bot.Builder.Planning
{
    public class SequenceDialog : PlanningDialog
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
