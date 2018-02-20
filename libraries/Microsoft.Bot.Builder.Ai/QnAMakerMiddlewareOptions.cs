namespace Microsoft.Bot.Builder.Ai
{
    /// <summary>
    /// Options to alter the default behaviour of the QnA Maker Middleware
    /// </summary>
    public class QnAMakerMiddlewareOptions
    {
        /// <summary>
        /// If true then routing of the activity will be stopped when an answer is successfully returned by the QnA Maker Middleware
        /// </summary>
        public bool EndActivityRoutingOnAnswer { get; set; }

        /// <summary>
        /// If set this message will be output before the top answer returned from the service. e.g. "I think this answer might help you..."
        /// </summary>
        public string DefaultAnswerPrefixMessage { get; set; } 

        public QnAMakerMiddlewareOptions(bool endActivityRoutingOnAnswer = false, string defaultAnswerPrefixMessage = null)
        {
            EndActivityRoutingOnAnswer = endActivityRoutingOnAnswer;
            DefaultAnswerPrefixMessage = defaultAnswerPrefixMessage;
        }
    }
}
