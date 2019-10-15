// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Skills
{
    /// <summary>
    /// Manages encoding ConversationId and ServiceUrl into packaged string for skill's conversation Id.
    /// </summary>
    internal class SkillConversation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SkillConversation"/> class.
        /// </summary>
        /// <param name="skillConversationId">packed skill conversationId to unpack.</param>
        public SkillConversation(string skillConversationId)
        {
            var parts = JsonConvert.DeserializeObject<string[]>(Encoding.UTF8.GetString(Convert.FromBase64String(skillConversationId)));
            ConversationId = parts[0];
            ServiceUrl = parts[1];
        }

        public string ConversationId { get; set; }

        public string ServiceUrl { get; set; }

        /// <summary>
        /// Get packed skill conversationId.
        /// </summary>
        /// <returns>packed conversationId.</returns>
        public string GetSkillConversationId()
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new[]
            {
                ConversationId,
                ServiceUrl,
            })));
        }
    }
}
