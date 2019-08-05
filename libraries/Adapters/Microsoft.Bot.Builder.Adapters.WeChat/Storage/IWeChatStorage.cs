// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Storage
{
    public interface IWeChatStorage<T>
    {
        Task SaveAsync(string key, T value, CancellationToken cancellationToken = default(CancellationToken));

        Task<T> GetAsync(string key, CancellationToken cancellationToken = default(CancellationToken));
    }
}
