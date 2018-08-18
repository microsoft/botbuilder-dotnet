using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Ai.LanguageGeneration.Engine
{
    internal interface IEntityInspector
    {
        Task<IDictionary<string, object>> InspectAsync(IDictionary<string, object> entities);
    }
}
