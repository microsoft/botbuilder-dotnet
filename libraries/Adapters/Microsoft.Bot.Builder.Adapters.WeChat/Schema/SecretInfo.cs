namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema
{
    /// <summary>
    /// Secret info store the parameter used to verify the message from WeChat and decrypt message content.
    /// </summary>
    public class SecretInfo
    {
        /// <summary>
        /// Gets or Sets signature from WeChat update webhook request.
        /// </summary>
        /// <value>
        /// signature from WeChat update webhook request.
        /// </value>
        public string Signature { get; set; }

        /// <summary>
        /// Gets or Sets signature from WeChat message request.
        /// </summary>
        /// <value>
        /// Signature from WeChat message request.
        /// </value>
        public string Msg_Signature { get; set; }

        /// <summary>
        /// Gets or Sets timestamp.
        /// </summary>
        /// <value>
        /// Timestamp of the request parameter.
        /// </value>
        public string Timestamp { get; set; }

        /// <summary>
        /// Gets or Sets nonce.
        /// </summary>
        /// <value>
        /// Nonce of the request parameter.
        /// </value>
        public string Nonce { get; set; }

        /// <summary>
        /// Gets or Sets token.
        /// </summary>
        /// <value>
        /// Token from the request parameter.
        /// </value>
        public string Token { get; set; }

        /// <summary>
        /// Gets or Sets endcoding aes key.
        /// </summary>
        /// <value>
        /// EncodingAESKey from appsetings.
        /// </value>
        public string EncodingAESKey { get; set; }

        /// <summary>
        /// Gets or Sets WeChat app id.
        /// </summary>
        /// <value>
        /// WeChat app id.
        /// </value>
        public string AppId { get; set; }
    }
}
