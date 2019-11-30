// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Skills;
using Newtonsoft.Json;

namespace DialogRootBot
{
    public class SkillConversationIdFactory
        : ISkillConversationIdFactory
    {
        public string CreateSkillConversationId(string conversationId, string serviceUrl)
        {
            if (string.IsNullOrWhiteSpace(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            if (serviceUrl == null)
            {
                throw new ArgumentNullException(nameof(serviceUrl));
            }

            var json = JsonConvert.SerializeObject(new[]
            {
                conversationId,
                serviceUrl
            });
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }

        public (string, string) GetConversationInfo(string conversationId)
        {
            var parts = JsonConvert.DeserializeObject<string[]>(Encoding.UTF8.GetString(Convert.FromBase64String(conversationId)));
            return (parts[0], parts[1]);
        }
    }
}
