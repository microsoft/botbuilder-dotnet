using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Ai.LanguageGeneration.Engine
{
    internal interface IResponseGenerator
    {
        Task<ICompositeResponse> GenerateResponseAsync(ICompositeRequest request);
    }
}
