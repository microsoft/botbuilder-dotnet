namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class ExpressionRef
    {
        public ExpressionRef(string expression, int line, string resource)
        {
            this.Expression = expression;
            this.Line = line;
            this.Resource = resource;
        }

        public string Expression { get; set; }

        public int Line { get; set; }

        public string Resource { get; set; }

        public override string ToString()
        {
            return Expression;
        }
    }
}
