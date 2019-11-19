using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Skills
{
    public class SkillDialogOption
    {
        public string TargetAction { get; set; }

        public Dictionary<string, object> Entities { get; } = new Dictionary<string, object>();
    }
}
