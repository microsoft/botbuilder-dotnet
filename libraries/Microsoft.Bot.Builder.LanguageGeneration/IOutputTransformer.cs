using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public interface IOutputTransformer
    {
        string Transform(string orignial, OutputTransformationContext context);
    }

    public class OutputTransformationContext
    { 
        public List<string> History { get; set; }
        public Dictionary<string, object> Properties { get; set; }
    }
}
