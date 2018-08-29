using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    internal interface IActivityComponentInspector
    {
        IList<string> Inspect(Activity activity);
    }
}
