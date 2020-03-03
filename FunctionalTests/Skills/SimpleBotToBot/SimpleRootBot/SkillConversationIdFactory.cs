// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.BotBuilderSamples.SimpleRootBot
{
    /// <summary>
    /// A <see cref="SkillConversationIdFactory"/> that uses an in memory <see cref="ConcurrentDictionary{TKey,TValue}"/>
    /// to store and retrieve <see cref="ConversationReference"/> instances.
    /// </summary>
    public class SkillConversationIdFactory : SkillConversationIdFactoryBase
    {
        private readonly ConcurrentDictionary<string, SkillConversationReference> _conversationRefs = new ConcurrentDictionary<string, SkillConversationReference>();

        public override Task<string> CreateSkillConversationIdAsync(SkillConversationIdFactoryOptions options, CancellationToken cancellationToken)
        {
            var skillConversationReference = new SkillConversationReference
            {
                ConversationReference = options.Activity.GetConversationReference(),
                OAuthScope = options.FromBotOAuthScope
            };
            var crJson = JsonConvert.SerializeObject(skillConversationReference);
            var key = $"{skillConversationReference.ConversationReference.Conversation.Id}-{skillConversationReference.ConversationReference.ChannelId}-skillconvo";
            _conversationRefs.GetOrAdd(key, skillConversationReference);
            return Task.FromResult(key);
        }

        public override Task<SkillConversationReference> GetSkillConversationReferenceAsync(string skillConversationId, CancellationToken cancellationToken)
        {
            var conversationReference = _conversationRefs[skillConversationId];
            return Task.FromResult(conversationReference);
        }

        public override Task DeleteConversationReferenceAsync(string skillConversationId, CancellationToken cancellationToken)
        {
            _conversationRefs.TryRemove(skillConversationId, out _);
            return Task.CompletedTask;
        }
    }
}
