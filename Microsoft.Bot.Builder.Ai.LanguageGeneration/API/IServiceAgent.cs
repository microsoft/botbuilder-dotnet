using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DialogFoundation.Backend.LG;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.API
{
    public interface IServiceAgent
    {
        string Generate(LGRequest request);

        Task<string> GenerateAsync(LGRequest request);
    }
}
