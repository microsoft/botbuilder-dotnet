using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Ai.LanguageGeneration.Engine
{
    internal interface IResolverPipeline
    {
        Task ExecuteAsync(Activity activity, IDictionary<string, object> entities);
    }
}
