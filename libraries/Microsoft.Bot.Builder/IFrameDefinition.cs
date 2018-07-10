using System.Collections.Generic;

namespace Microsoft.Bot.Builder
{
    public interface IFrameDefinition
    {
        string Scope { get; set; }

        string NameSpace { get; set; }

        List<ISlotDefinition> SlotDefinitions { get; set; }
    }
}
