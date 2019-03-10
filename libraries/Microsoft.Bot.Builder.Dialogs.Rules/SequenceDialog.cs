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
        public override List<IDialog> Steps { get; set; }

        public override List<IRule> Rules
        {
            get
            {
                return new List<IRule>() { new FallbackRule(steps) }.Concat(base.Rules).ToList();
            }
            set
            {
                base.Rules = value;
            }
        }

        public SequenceDialog(string dialogId = null, List<IDialog> steps = null)
            : base(dialogId)
        {          
        }
    }
}
