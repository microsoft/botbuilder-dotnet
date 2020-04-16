using System;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public enum LGLineBreakStyle
    {
        /// <summary>
        /// Default mode
        /// </summary>
        Default,

        /// <summary>
        /// Markdown mode
        /// </summary>
        Markdown,
    }

    /// <summary>
    /// Options for evaluation of LG template <see cref="EvaluationOptions"/> class.
    /// </summary>
    public class EvaluationOptions
    {
        public EvaluationOptions()
        {
            this.StrictMode = null;
            this.NullSubstitution = null;
            this.LineBreakStyle = null;
        }

        public EvaluationOptions(EvaluationOptions opt)
        {
            this.StrictMode = opt.StrictMode;
            this.NullSubstitution = opt.NullSubstitution;
            this.LineBreakStyle = opt.LineBreakStyle;
        }

        public LGLineBreakStyle? LineBreakStyle { get; set; } = null;

        public bool? StrictMode { get; set; } = null;

        public Func<string, object> NullSubstitution { get; set; } = null;
    }
}
