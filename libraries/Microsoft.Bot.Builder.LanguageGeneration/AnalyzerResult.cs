using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class AnalyzerResult
    {
        public AnalyzerResult(List<string> variables = null, List<string> templateReferences = null)
        {
            this.Variables = (variables ?? new List<string>()).Distinct().ToList();
            this.TemplateReferences = (templateReferences ?? new List<string>()).Distinct().ToList();
        }

        public List<string> Variables { get; set; }

        public List<string> TemplateReferences { get; set; }

        public AnalyzerResult Union(AnalyzerResult outputItem)
        {
            this.Variables = this.Variables.Union(outputItem.Variables).ToList();
            this.TemplateReferences = this.TemplateReferences.Union(outputItem.TemplateReferences).ToList();
            return this;
        }
    }
}
