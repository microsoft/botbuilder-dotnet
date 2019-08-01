namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    /// <summary>
    /// Some default value used when mapping the message from WeChat to Activity.
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// Max size for a single WeChat response message.
        /// </summary>
        public const int MaxSingleMessageLength = 2048;

        /// <summary>
        /// Max size for one request to WeChat.
        /// </summary>
        public const int MaxTotalMessageLength = 20480;

        /// <summary>
        /// Default service url for WeChat channel.
        /// </summary>
        public const string ServiceUrl = "";

        /// <summary>
        /// Default value for WeChat news message.
        /// </summary>
        public const string DefaultContentUrl = "https://dev.botframework.com";

        /// <summary>
        /// Key to get all response from bot in a single turn.
        /// </summary>
        public const string TurnResponseKey = "turnResponse";

        /// <summary>
        /// Default error message when bot adapter failed.
        /// </summary>
        public const string DefaultErrorMessage = "Sorry, something went wrong.";

        /// <summary>
        /// New line string.
        /// </summary>
        public const string NewLine = "\r\n";
    }
}
