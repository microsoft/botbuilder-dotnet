// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Tests
{
    public class CallCountingMiddleware : IMiddleware
    {
        public int Calls { get; set; }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken)
        {
            Calls++;
            await next(cancellationToken);
        }
    }
}
