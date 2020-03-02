// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.TestProtocol
{
    public class MyConversationIdFactory : SkillHostConversationIdFactoryBase
    {
        private readonly ConcurrentDictionary<string, (ConversationReference, string)> _conversationRefs = new ConcurrentDictionary<string, (ConversationReference, string)>();

        public override Task<string> CreateSkillConversationIdAsync(string originatingAudience, string fromBotId, Activity activity, BotFrameworkSkill botFrameworkSkill, CancellationToken cancellationToken)
        {
            var key = (activity.Conversation.Id + activity.ServiceUrl).GetHashCode().ToString(CultureInfo.InvariantCulture);
            _conversationRefs.GetOrAdd(key, (activity.GetConversationReference(), originatingAudience));
            return Task.FromResult(key);
        }

        public override Task<(ConversationReference, string)> GetConversationReferenceWithAudienceAsync(string skillConversationId, CancellationToken cancellationToken)
        {
            var conversationReference = _conversationRefs[skillConversationId].Item1;
            return Task.FromResult((conversationReference, _conversationRefs[skillConversationId].Item2));
        }

        public override Task DeleteConversationReferenceAsync(string skillConversationId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
