using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Ai.LanguageGeneration.API;

namespace Microsoft.Bot.Builder.Ai.LanguageGeneration.Engine
{
    internal interface IResponseGenerator
    {
        Task<ICompositeResponse> GenerateResponseAsync(ICompositeRequest compositeRequest, ServiceAgent serviceAgent);
    }
}
