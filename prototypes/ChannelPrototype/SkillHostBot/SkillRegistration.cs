namespace SkillHost
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
