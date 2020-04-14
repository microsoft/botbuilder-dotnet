using AdaptiveExpressions;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class LGOptions : Options 
    {
        public LGOptions()
        {
            this.StrictMode = false;
            this.NullSubstitution = null;
            this.LineBreakStyle = "default";
        }

        public LGOptions(LGOptions opt)
        {
            this.StrictMode = opt.StrictMode;
            this.NullSubstitution = opt.NullSubstitution;
            this.LineBreakStyle = opt.LineBreakStyle;
        }

        public string LineBreakStyle { get; set; } = "default";
    }
}
