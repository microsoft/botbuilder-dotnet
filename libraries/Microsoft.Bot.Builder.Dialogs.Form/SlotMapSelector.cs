using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive;

namespace Microsoft.Bot.Builder.Dialogs.Form
{
    public class SlotMapSelector : IRuleSelector
    {
        public void Initialize(IEnumerable<IRule> rules, bool evaluate = true)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<int>> Select(SequenceContext context, CancellationToken cancel = default(CancellationToken))
        {
            throw new NotImplementedException();
        }
    }
}
