// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Tests
{
    internal class ConnectorFactoryBot : IBot
    {
        public object Authorization { get; internal set; }

        public IAsyncEnumerable<object> Identity { get; internal set; }

        public IAsyncEnumerable<object> UserTokenClient { get; internal set; }

        public Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}
