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
        /// Gets or Sets echo string from WeChat message request.
        /// </summary>
        /// <value>
        /// Echo string from WeChat message request.
        /// </value>
        [FromQuery(Name = "echostr")]
        public string EchoString { get; set; }

        /// <summary>
        /// Gets or Sets timestamp.
        /// </summary>
        /// <value>
        /// Timestamp of the request parameter.
        /// </value>
        [FromQuery(Name = "timestamp")]
        public string Timestamp { get; set; }

        /// <summary>
        /// Gets or Sets nonce.
        /// </summary>
        /// <value>
        /// Nonce of the request parameter.
        /// </value>
        [FromQuery(Name = "nonce")]
        public string Nonce { get; set; }
    }
}
