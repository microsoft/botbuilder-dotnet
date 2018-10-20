// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.TestBot.WebApi
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
