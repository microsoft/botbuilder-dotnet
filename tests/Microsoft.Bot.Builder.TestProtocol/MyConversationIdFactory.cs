// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Skills;

namespace Microsoft.Bot.Builder.TestProtocol
{
    public class MyConversationIdFactory : ISkillConversationIdFactory
    {
        private readonly ConcurrentDictionary<string, (string, string)> _backwardXref;
        private readonly ConcurrentDictionary<string, string> _forwardXref;

        public MyConversationIdFactory()
        {
            _forwardXref = new ConcurrentDictionary<string, string>();
            _backwardXref = new ConcurrentDictionary<string, (string, string)>();
        }

        public Task<string> CreateSkillConversationIdAsync(string conversationId, string serviceUrl, CancellationToken cancellationToken)
        {
            var result = _forwardXref.GetOrAdd(conversationId, key => { return Guid.NewGuid().ToString(); });
            _backwardXref[result] = (conversationId, serviceUrl);
            return Task.FromResult(result);
        }

        public Task<(string conversationId, string serviceUrl)> GetConversationInfoAsync(string encodedConversationId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_backwardXref[encodedConversationId]);
        }
    }
}
