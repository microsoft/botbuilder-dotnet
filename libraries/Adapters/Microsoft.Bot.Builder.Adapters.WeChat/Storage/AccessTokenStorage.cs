// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Storage
{
    internal class AccessTokenStorage : IWeChatStorage<WeChatAccessToken>, IDisposable
    {
        private readonly IStorage _storage;
        private SemaphoreSlim _semaphore;

        public AccessTokenStorage(IStorage storage)
        {
            _storage = storage;
            _semaphore = new SemaphoreSlim(1);
        }

        public async Task SaveAsync(string key, WeChatAccessToken value, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);
                var dict = new Dictionary<string, object>
                {
                    { key, value },
                };
                await _storage.WriteAsync(dict, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<WeChatAccessToken> GetAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);
                var keys = new string[] { key };
                var result = await _storage.ReadAsync<WeChatAccessToken>(keys, cancellationToken).ConfigureAwait(false);
                result.TryGetValue(key, out var wechatResult);
                if (wechatResult == null || wechatResult.ExpireTime.ToUnixTimeSeconds() <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    return null;
                }

                return wechatResult;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task DeleteAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);
                var keys = new string[] { key };
                await _storage.DeleteAsync(keys, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (_semaphore != null)
                {
                    _semaphore.Dispose();
                    _semaphore = null;
                }
            }
        }
    }
}
