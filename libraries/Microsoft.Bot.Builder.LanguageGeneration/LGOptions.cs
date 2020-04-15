using AdaptiveExpressions;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public enum LGLineBreakStyle
    {
        /// <summary>
        /// Default mode
        /// </summary>
        DEFAULT,

        /// <summary>
        /// Markdown mode
        /// </summary>
        MARKDOWN,
    }

    /// <summary>
    /// Options for evaluation of LG template <see cref="LGOptions"/> class.
    /// </summary>
    public class LGOptions : Options 
    {
        public LGOptions()
        {
            this.StrictMode = false;
            this.NullSubstitution = null;
            this.LineBreakStyle = LGLineBreakStyle.DEFAULT;
        }

        public LGOptions(LGOptions opt)
        {
            this.StrictMode = opt.StrictMode;
            this.NullSubstitution = opt.NullSubstitution;
            this.LineBreakStyle = opt.LineBreakStyle;
        }

        public LGLineBreakStyle LineBreakStyle { get; set; } = LGLineBreakStyle.DEFAULT;
    }
}
