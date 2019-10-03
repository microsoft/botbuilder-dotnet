using System;
using System.Linq;
using System.Threading.Tasks;

namespace ChannelPrototype.Controllers
{
    public class SkillRegistration
    {
        /// <summary>
        /// Id of the skill.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// AppId of the skill.
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// /api/messages endpoint for the skill.
        /// </summary>
        public string ServiceUrl { get; set; }
    }
}
