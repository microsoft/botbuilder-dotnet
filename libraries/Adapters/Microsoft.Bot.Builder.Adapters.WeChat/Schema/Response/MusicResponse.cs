namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Response
{
    public class MusicResponse : ResponseMessage
    {
        public MusicResponse()
        {
        }

        public MusicResponse(Music music)
        {
            Music = music;
        }

        public override ResponseMessageType MsgType => ResponseMessageType.Music;

        public Music Music { get; set; }
    }
}
