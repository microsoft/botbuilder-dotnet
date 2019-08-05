// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema;

namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    public sealed class AccessTokenStorage : IWeChatStorage<WeChatAccessToken>
    {
        public static readonly AccessTokenStorage Instance = new AccessTokenStorage();
        private readonly IStorage _storage;

        public AccessTokenStorage()
        {
            _storage = new MemoryStorage();
        }

        public AccessTokenStorage(IStorage storage)
        {
            _storage = storage;
        }

        public async Task SaveAsync(string key, WeChatAccessToken value, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dict = new Dictionary<string, object>
            {
                { key, value },
            };
            await _storage.WriteAsync(dict, cancellationToken);
        }

        public async Task<WeChatAccessToken> GetAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            var keys = new string[] { key };
            var result = await _storage.ReadAsync<WeChatAccessToken>(keys, cancellationToken);
            result.TryGetValue(key, out var wechatResult);
            return wechatResult;
        }

        public async Task DeleteAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            var keys = new string[] { key };
            await _storage.DeleteAsync(keys, cancellationToken);
        }
    }
}
