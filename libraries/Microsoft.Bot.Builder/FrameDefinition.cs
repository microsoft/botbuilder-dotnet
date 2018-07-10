using System.Collections.Generic;

namespace Microsoft.Bot.Builder
{
    public class FrameDefinition
    {
        public string Scope { get; set; }

        public string NameSpace { get; set; }

        public List<SlotDefinition> SlotDefinitions { get; set; } = new List<SlotDefinition>();
    }
}
