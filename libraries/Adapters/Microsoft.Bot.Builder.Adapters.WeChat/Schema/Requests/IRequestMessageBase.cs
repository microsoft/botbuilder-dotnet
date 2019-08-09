// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests
{
    public interface IRequestMessageBase
    {
        /// <summary>
        /// Gets MsgType.
        /// </summary>
        /// <value>
        /// Message type of the request.
        /// </value>
        string MsgType { get; }

        string Encrypt { get; set; }

        /// <summary>
        /// Gets or sets ToUserName.
        /// </summary>
        /// <value>
        /// Recipient OpenId from WeChat.
        /// </value>
        string ToUserName { get; set; }

        /// <summary>
        /// Gets or sets FromUserName.
        /// </summary>
        /// <value>
        /// Sender OpenId from WeChat.
        /// </value>
        string FromUserName { get; set; }

        /// <summary>
        /// Gets or sets CreateTime.
        /// </summary>
        /// <value>
        /// Message Created time.
        /// </value>
        long CreateTime { get; set; }
    }
}
