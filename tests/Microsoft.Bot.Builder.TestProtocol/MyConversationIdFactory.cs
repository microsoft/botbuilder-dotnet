// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Skills;

namespace Microsoft.Bot.Builder.TestProtocol
{
    public class MyConversationIdFactory : SkillConversationIdFactoryBase
    {
        private readonly ConcurrentDictionary<string, SkillConversationReference> _conversationRefs = new ConcurrentDictionary<string, SkillConversationReference>();

        public override Task<string> CreateSkillConversationIdAsync(SkillConversationIdFactoryOptions options, CancellationToken cancellationToken = default)
        {
            var key = (options.Activity.Conversation.Id + options.Activity.ServiceUrl).GetHashCode().ToString(CultureInfo.InvariantCulture);
            _conversationRefs.GetOrAdd(key, new SkillConversationReference
            {
                ConversationReference = options.Activity.GetConversationReference(),
                OAuthScope = options.FromBotOAuthScope
            });
            return Task.FromResult(key);
        }

        public override Task<SkillConversationReference> GetSkillConversationReferenceAsync(string skillConversationId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_conversationRefs[skillConversationId]);
        }

        public override Task DeleteConversationReferenceAsync(string skillConversationId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
