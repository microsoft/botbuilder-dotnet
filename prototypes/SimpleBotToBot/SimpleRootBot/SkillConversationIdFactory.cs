// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Newtonsoft.Json;

namespace SimpleRootBot
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

            var jsonString = JsonConvert.SerializeObject(new[]
            {
                conversationId,
                serviceUrl
            });

            //return HttpUtility.UrlEncode(jsonString);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonString));
        }

        public (string, string) GetConversationInfo(string conversationId)
        {
            //var jsonString = HttpUtility.UrlDecode(conversationId);
            var jsonString = Encoding.UTF8.GetString(Convert.FromBase64String(conversationId));
            var parts = JsonConvert.DeserializeObject<string[]>(jsonString);
            return (parts[0], parts[1]);
        }
    }
}
