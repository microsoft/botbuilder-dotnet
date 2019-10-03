namespace SkillHost
{
    public class SkillRegistration
    {
        /// <summary>
        /// Gets or sets id of the skill.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets appId of the skill.
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// Gets or sets /api/messages endpoint for the skill.
        /// </summary>
        public string ServiceUrl { get; set; }
    }
}
