using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.TestBot
{
    public class InteceptorAdapter : IAdapterIntegration
    {
        private IAdapterIntegration _innerAdapter;

        public InteceptorAdapter(IAdapterIntegration innerAdapter)
        {
            _innerAdapter = innerAdapter;
        }

        public Task<InvokeResponse> ProcessActivityAsync(string authHeader, Activity activity, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            return _innerAdapter.ProcessActivityAsync(authHeader, activity, callback, cancellationToken);
        }
    }
}
