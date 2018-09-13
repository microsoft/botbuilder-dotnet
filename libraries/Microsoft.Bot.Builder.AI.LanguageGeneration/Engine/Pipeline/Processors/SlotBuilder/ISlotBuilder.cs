using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    internal interface ISlotBuilder
    {
        IList<Slot> BuildSlots(Activity activity, IDictionary<string, object> entities);
    }
}
