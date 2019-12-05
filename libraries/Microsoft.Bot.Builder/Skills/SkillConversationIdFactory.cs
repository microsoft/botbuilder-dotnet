// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Skills
{
    /// <summary>
    /// A <see cref="ISkillConversationIdFactory"/> that uses <see cref="IStorage"/> to store and retrieve <see cref="ConversationReference"/> instances.
    /// </summary>
    public class SkillConversationIdFactory : ISkillConversationIdFactory
    {
        private readonly IStorage _storage;

        public SkillConversationIdFactory(IStorage storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public async Task<string> CreateSkillConversationIdAsync(ConversationReference conversationReference, CancellationToken cancellationToken)
        {
            if (conversationReference == null)
            {
                throw new ArgumentNullException(nameof(conversationReference));
            }

            if (string.IsNullOrWhiteSpace(conversationReference.Conversation.Id))
            {
                throw new NullReferenceException($"ConversationId in {nameof(conversationReference)} can't be null.");
            }

            if (string.IsNullOrWhiteSpace(conversationReference.ServiceUrl))
            {
                throw new NullReferenceException($"ServiceUrl in {nameof(conversationReference)} can't be null.");
            }

            var storageKey = (conversationReference.Conversation.Id + conversationReference.ServiceUrl).GetHashCode().ToString(CultureInfo.InvariantCulture);
            var skillConversationInfo = new Dictionary<string, object> { { storageKey, JObject.FromObject(conversationReference) } };
            await _storage.WriteAsync(skillConversationInfo, cancellationToken).ConfigureAwait(false);

            return storageKey;
        }

        public async Task<ConversationReference> GetConversationReferenceAsync(string skillConversationId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(skillConversationId))
            {
                throw new ArgumentNullException(nameof(skillConversationId));
            }

            var skillConversationInfo = await _storage.ReadAsync(new[] { skillConversationId }, cancellationToken).ConfigureAwait(false);
            if (!skillConversationInfo.Any())
            {
                throw new InvalidOperationException($"Unable to find skill conversation information for skillConversationId {skillConversationId}");
            }

            var conversationInfo = ((JObject)skillConversationInfo[skillConversationId]).ToObject<ConversationReference>();
            return conversationInfo;
        }
    }
}
