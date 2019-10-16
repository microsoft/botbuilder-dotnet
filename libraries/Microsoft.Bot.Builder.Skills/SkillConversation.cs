// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#pragma warning disable CA2208 // Instantiate argument exceptions correctly

using System;
using System.Text;
using System.Web;
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
        public SkillConversation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillConversation"/> class.
        /// </summary>
        /// <param name="packedConversationId">packed skill conversationId to unpack.</param>
        public SkillConversation(string packedConversationId)
        {
            var parts = JsonConvert.DeserializeObject<string[]>(Encoding.UTF8.GetString(Convert.FromBase64String(packedConversationId)));
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
            if (string.IsNullOrEmpty(ConversationId))
            {
                throw new ArgumentNullException(nameof(ConversationId));
            }

            if (string.IsNullOrEmpty(ServiceUrl))
            {
                throw new ArgumentNullException(nameof(ServiceUrl));
            }
             
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new[]
            {
                ConversationId,
                ServiceUrl,
            })));
        }
    }
}
