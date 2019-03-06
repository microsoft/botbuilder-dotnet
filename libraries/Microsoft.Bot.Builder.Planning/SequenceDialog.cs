using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Planning.Rules;

namespace Microsoft.Bot.Builder.Planning
{
    public class SequenceDialog : PlanningDialog//, IDialogDependencies
    {
        public SequenceDialog(string dialogId = null, List<IDialog> steps = null)
            : base(dialogId)
        {
            AddRule(new[] { new FallbackRule(steps ?? new List<IDialog>())});
        }
    }
}
