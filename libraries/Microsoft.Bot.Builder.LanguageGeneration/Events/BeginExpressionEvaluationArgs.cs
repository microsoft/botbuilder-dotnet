namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class BeginExpressionEvaluationArgs : ExpressionEventArgs
    {
        public string Type { get; } = EventTypes.BeginExpressionEvaluation;

        public string Expression { get; set; }
    }
}
