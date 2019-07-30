namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    /// <summary>
    /// Some default value used when mapping the message from WeChat to Activity.
    /// </summary>
    public class Constants
    {
        // TODO: should be change depend on documents.
        public const int MaxSingleMessageLength = 2048;
        public const int MaxTotalMessageLength = 20480;
        public const string ServiceUrl = "";
        public const string DefaultContentUrl = "https://dev.botframework.com";
        public const string ChannelId = "wechat";
        public const string TurnResponseKey = "turnResponse";
        public const string DefaultErrorMessage = "Sorry, something went wrong.";
        public const string NewLine = "\r\n";
    }
}
