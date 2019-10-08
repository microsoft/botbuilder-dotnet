using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace SkillHost.Controllers
{
    /// <summary>
    /// Manages encoding ConverstionId and ServiceUrl into packaged string for skill's conversation Id.
    /// </summary>
    public class SkillConversation
    {
        public SkillConversation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillConversation"/> class.
        /// </summary>
        /// <param name="skillConverationId">packed skill conversationId to unpack.</param>
        public SkillConversation(string skillConverationId)
        {
            var parts = JsonConvert.DeserializeObject<string[]>(Encoding.UTF8.GetString(Convert.FromBase64String(skillConverationId)));
            ConversationId = parts[0];
            ServiceUrl = parts[1];
        }

        public string ConversationId { get; set; }

        public string ServiceUrl { get; set; }

        /// <summary>
        /// Get packed skill conversationId.
        /// </summary>
        /// <returns>packed conversationId.</returns>
        public string GetSkillConverationId()
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new string[] { this.ConversationId, this.ServiceUrl })));
        }
    }
}
