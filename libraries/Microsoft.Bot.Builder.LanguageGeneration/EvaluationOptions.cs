using System;

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
    public class EvaluationOptions
    {
        public EvaluationOptions()
        {
            this.StrictMode = false;
            this.NullSubstitution = null;
            this.LineBreakStyle = LGLineBreakStyle.DEFAULT;
        }

        public EvaluationOptions(EvaluationOptions opt)
        {
            this.StrictMode = opt.StrictMode;
            this.NullSubstitution = opt.NullSubstitution;
            this.LineBreakStyle = opt.LineBreakStyle;
        }

        public LGLineBreakStyle LineBreakStyle { get; set; } = LGLineBreakStyle.DEFAULT;

        public bool StrictMode { get; set; } = false;

        public Func<string, object> NullSubstitution { get; set; } = null;
    }
}
