namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema
{
    public class SecretInfo
    {
        public string Signature { get; set; }

        public string Msg_Signature { get; set; }

        public string Timestamp { get; set; }

        public string Nonce { get; set; }

        public string Token { get; set; }

        public string EncodingAESKey { get; set; }

        public string AppId { get; set; }

        public void SetSecretInfo(string token, string encodingAESKey, string appId)
        {
            this.AppId = appId;
            this.EncodingAESKey = encodingAESKey;
            this.Token = token;
        }
    }
}
