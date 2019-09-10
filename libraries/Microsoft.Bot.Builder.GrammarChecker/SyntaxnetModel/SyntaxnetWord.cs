using Microsoft.Bot.Builder.GrammarChecker.SyntaxFeatures;

namespace Microsoft.Bot.Builder.GrammarChecker.SyntaxnetModel
{
    public class SyntaxnetWord
    {
        public SyntaxnetWord()
        {
            Word = string.Empty;
            WordHead = -1;
            WordIndex = -1;           
        }

        public int WordIndex { get; set; }

        public int WordHead { get; set; }

        public string Word { get; set; }

        public BasicPosTag WordPOS { get; set; }

        public NounPosTag NounPosTag { get; set; }

        public DependencyTag WordDependency { get; set; }
    }
}
