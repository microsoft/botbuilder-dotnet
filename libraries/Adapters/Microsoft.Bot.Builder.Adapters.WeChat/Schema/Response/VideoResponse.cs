namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Response
{
    public class VideoResponse : ResponseMessage
    {
        public VideoResponse()
        {
        }

        public VideoResponse(Video video)
        {
            Video = video;
        }

        public VideoResponse(string mediaId, string title = null, string description = null)
        {
            Video = new Video(mediaId, title, description);
        }

        public override ResponseMessageType MsgType => ResponseMessageType.Video;

        public Video Video { get; set; }
    }
}
