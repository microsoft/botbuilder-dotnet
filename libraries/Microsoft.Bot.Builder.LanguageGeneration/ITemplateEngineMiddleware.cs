using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public interface ITemplateEngineMiddleware
    {
        string Replace(string previous, TemplateReplacementContext context);
    }

    public class TemplateReplacementContext
    {
        public List<string> History { get; set; }

        public Dictionary<string, object> Properties { get; set; }
    }

}
