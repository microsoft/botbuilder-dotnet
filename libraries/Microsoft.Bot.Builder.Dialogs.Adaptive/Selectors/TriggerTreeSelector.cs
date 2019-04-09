using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors
{
    public class TriggerTreeSelector : IRuleSelector
    {
        public Task<IReadOnlyList<int>> Candidates(DialogContext context, CancellationToken cancel)
        {
            throw new NotImplementedException();
        }

        public Task Initialize(DialogContext context, IEnumerable<IRule> rules, CancellationToken cancel)
        {
            throw new NotImplementedException();
        }

        public Task<int> Select(DialogContext context, CancellationToken cancel)
        {
            throw new NotImplementedException();
        }
    }
}
