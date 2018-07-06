using System.Collections.Generic;

namespace Microsoft.Bot.Builder
{
    public interface IFrameDefinition
    {
        string FrameScope { get; set; }

        string NameSpace { get; set; }

        List<ISlotDefinition> Slots { get; set; }
    }
}
