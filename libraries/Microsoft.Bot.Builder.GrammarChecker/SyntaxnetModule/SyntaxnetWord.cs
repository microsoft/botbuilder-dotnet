using Microsoft.Bot.Builder.GrammarChecker.SyntaxFeatures;

namespace Microsoft.Bot.Builder.GrammarChecker.SyntaxnetModule
{
    public class SyntaxnetWord
    {
        private string m_word;
        private int m_wordIndex;
        private POSTag m_WordPOS;
        private DependencyTag m_WordDependency;
        private int m_WordHead; //head word index
        
        public SyntaxnetWord()
        {
            m_word = string.Empty;
            m_WordHead = -1;
            m_wordIndex = -1;
            m_WordPOS = POSTag.OTHER;
            m_WordDependency = DependencyTag.other;
        }

        public int WordIndex
        {
            get => m_wordIndex;
            set => m_wordIndex = value;
        }

        public int WordHead
        {
            get => m_WordHead;
            set => m_WordHead = value;
        }

        public string Word
        {
            get => m_word;
            set => m_word = value;
        }

        public POSTag WordPOS
        {
            get => m_WordPOS;
            set => m_WordPOS = value;
        }

        public DependencyTag WordDependency
        {
            get => m_WordDependency;
            set => m_WordDependency = value;
        }
    }
}
