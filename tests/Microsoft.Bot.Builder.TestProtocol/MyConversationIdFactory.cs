// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.TestProtocol
{
    public class MyConversationIdFactory : SkillConversationIdFactoryBase
    {
        private readonly ConcurrentDictionary<string, string> _conversationRefs = new ConcurrentDictionary<string, string>();

        public override Task<string> CreateSkillConversationIdAsync(ConversationReference conversationReference, CancellationToken cancellationToken)
        {
            var crJson = JsonConvert.SerializeObject(conversationReference);
            var key = (conversationReference.Conversation.Id + conversationReference.ServiceUrl).GetHashCode().ToString(CultureInfo.InvariantCulture);
            _conversationRefs.GetOrAdd(key, crJson);
            return Task.FromResult(key);
        }

        public override Task<ConversationReference> GetConversationReferenceAsync(string skillConversationId, CancellationToken cancellationToken)
        {
            var conversationReference = JsonConvert.DeserializeObject<ConversationReference>(_conversationRefs[skillConversationId]);
            return Task.FromResult(conversationReference);
        }

        public override Task DeleteConversationReferenceAsync(string skillConversationId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
