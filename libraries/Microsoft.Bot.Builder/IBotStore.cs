// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public interface IBotStore
    {
        Task<bool> LoadAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken));
        Task<bool> TrySaveChangesAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken));
    }
}
