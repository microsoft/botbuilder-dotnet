namespace Microsoft.Bot.Builder.Ai.QnA
{
    /// <summary>
    /// Options to alter the default behaviour of the QnA Maker Middleware
    /// extending the QnAMakerOptions used on the QnAMaker client
    /// </summary>
    public class QnAMakerMiddlewareOptions : QnAMakerOptions
    {
        /// <summary>
        /// If true then routing of the activity will be stopped when an answer is 
        /// successfully returned by the QnA Maker Middleware.
        /// 
        /// Defaults to false.
        /// </summary>
        public bool EndActivityRoutingOnAnswer { get; set; } = false;

        /// <summary>
        /// If set this message will be output before the top answer returned from 
        /// the service. e.g. "I think this answer might help you..."
        /// </summary>
        public string DefaultAnswerPrefixMessage { get; set; }

        public QnAMakerMiddlewareOptions()
        {
        }
    }
}
