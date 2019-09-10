namespace Microsoft.Bot.Builder.GrammarChecker.SyntaxnetModel
{
    public class SyntaxnetGrammarChecker : IGrammarChecker
    {
        private IPosTagger posTagger;
        private IDependencyParser dependencyParser;
        private ICorrector corrector;
        private IGrammarChecker grammarChecker;

        public SyntaxnetGrammarChecker()
        {
            this.posTagger = new SyntaxnetPosTagger();
            this.dependencyParser = new SyntaxnetDependencyParser();
            this.corrector = new Corrector();
            this.grammarChecker = new GrammarChecker(this.posTagger, this.dependencyParser, this.corrector);
        }

        public string CheckText(string text)
        {
            return this.grammarChecker.CheckText(text);
        }
    }
}
