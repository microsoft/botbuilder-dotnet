// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Skills
{
    public class SkillConversationIdFactory : ISkillConversationIdFactory
    {
        private readonly IStorage _storage;

        public SkillConversationIdFactory(IStorage storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public async Task<string> CreateSkillConversationIdAsync(string callerConversationId, string serviceUrl, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(callerConversationId))
            {
                throw new ArgumentNullException(nameof(callerConversationId));
            }

            if (string.IsNullOrWhiteSpace(serviceUrl))
            {
                throw new ArgumentNullException(nameof(serviceUrl));
            }

            var conversationInfo = new SkillConversationInfo
            {
                CallerConversationId = callerConversationId,
                ServiceUrl = serviceUrl
            };

            // TODO: Change this to return the hash key once I fix OAuthPrompt (Gabo).
            //var jsonInfo = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(conversationInfo)));
            //var storageKey = jsonInfo.GetHashCode().ToString(CultureInfo.InvariantCulture);
            var storageKey = callerConversationId;
            var skillConversationInfo = new Dictionary<string, object> { { storageKey, JObject.FromObject(conversationInfo) } };
            await _storage.WriteAsync(skillConversationInfo, cancellationToken).ConfigureAwait(false);

            return storageKey;
        }

        public async Task<(string conversationId, string serviceUrl)> GetConversationInfoAsync(string skillConversationId, CancellationToken cancellationToken)
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

            var conversationInfo = ((JObject)skillConversationInfo[skillConversationId]).ToObject<SkillConversationInfo>();
            return (conversationInfo.CallerConversationId, conversationInfo.ServiceUrl);
        }

        private class SkillConversationInfo
        {
            public string CallerConversationId { get; set; }

            public string ServiceUrl { get; set; }
        }
    }
}
