using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.LanguageGeneration.API;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    internal interface IResponseGenerator
    {
        Task<ICompositeResponse> GenerateResponseAsync(ICompositeRequest compositeRequest, IServiceAgent serviceAgent);
    }
}
