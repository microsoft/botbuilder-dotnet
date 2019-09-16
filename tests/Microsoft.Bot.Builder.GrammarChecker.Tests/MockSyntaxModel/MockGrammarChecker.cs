namespace Microsoft.Bot.Builder.GrammarChecker.Tests
{ 
    public class MockGrammarChecker : IGrammarChecker
    {
        private IPosTagger posTagger;
        private IDependencyParser dependencyParser;
        private ICorrector corrector;
        private IGrammarChecker grammarChecker;

        public MockGrammarChecker()
        {
            this.posTagger = new MockPosTagger();
            this.dependencyParser = new MockDependencyParser();
            this.corrector = new Corrector();
            this.grammarChecker = new GrammarChecker(this.posTagger, this.dependencyParser, this.corrector);
        }

        public string CheckText(string text)
        {
            return this.grammarChecker.CheckText(text);
        }
    }
}
