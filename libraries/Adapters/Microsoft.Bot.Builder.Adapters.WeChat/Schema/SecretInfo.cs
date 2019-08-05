// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;

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
        [FromQuery(Name = "signature")]
        public string WebhookSignature { get; set; }

        /// <summary>
        /// Gets or Sets signature from WeChat message request.
        /// </summary>
        /// <value>
        /// Signature from WeChat message request.
        /// </value>
        [FromQuery(Name = "msg_signature")]
        public string MessageSignature { get; set; }

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
        /// EncodingAESKey fixed length of 43 characters, a-z, A-Z, 0-9 a total of 62 characters selected
        /// https://open.weixin.qq.com/cgi-bin/showdocument?action=dir_list&t=resource/res_list&verify=1&id=open1419318479&token=&lang=en_US.
        /// </value>
        public string EncodingAesKey { get; set; }

        /// <summary>
        /// Gets or Sets WeChat app id.
        /// </summary>
        /// <value>
        /// WeChat app id.
        /// </value>
        public string AppId { get; set; }
    }
}
