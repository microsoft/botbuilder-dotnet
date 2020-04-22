namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class BeginTemplateEvaluationArgs : LGEventArgs
    {
        public string Type { get; } = EventTypes.BeginTemplateEvaluation;

        public string TemplateName { get; set; }
    }
}
