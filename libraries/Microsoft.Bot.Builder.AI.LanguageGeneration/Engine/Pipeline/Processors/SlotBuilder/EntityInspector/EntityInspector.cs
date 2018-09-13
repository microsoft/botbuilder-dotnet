using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    internal class EntityInspector : IEntityInspector
    {
        public async Task<IDictionary<string, object>> InspectAsync(IDictionary<string, object> entities)
        {
            throw new NotImplementedException();
        }
    }
}
