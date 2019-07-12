namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Response
{
    public class VoiceResponse : ResponseMessage
    {
        public VoiceResponse(Voice voice)
        {
            Voice = voice;
        }

        public VoiceResponse(string mediaId)
        {
            Voice = new Voice(mediaId);
        }

        public override ResponseMessageType MsgType => ResponseMessageType.Voice;

        public Voice Voice { get; set; }
    }
}
