using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Ai.LanguageGeneration.Engine
{
    internal interface ISlotBuilder
    {
        Task<IList<Slot>> BuildSlotsAsync(Activity activity, IDictionary<string, object> entities);
    }
}
