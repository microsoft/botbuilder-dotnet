namespace Microsoft.Bot.Builder.Ai
{
    /// <summary>
    /// Options to alter the default behaviour of the QnA Maker Middleware
    /// extending the QnAMakerOptions used on the QnAMaker client
    /// </summary>
    public class QnAMakerMiddlewareOptions : QnAMakerOptions
    {
        /// <summary>
        /// If true then routing of the activity will be stopped when an answer is 
        /// successfully returned by the QnA Maker Middleware
        /// </summary>
        public bool EndActivityRoutingOnAnswer { get; set; }

        /// <summary>
        /// If set this message will be output before the top answer returned from 
        /// the service. e.g. "I think this answer might help you..."
        /// </summary>
        public string DefaultAnswerPrefixMessage { get; set; }

        public QnAMakerMiddlewareOptions()
        {
            // By default the middleware will continue routing of the activity
            EndActivityRoutingOnAnswer = false;
        }
    }
}
