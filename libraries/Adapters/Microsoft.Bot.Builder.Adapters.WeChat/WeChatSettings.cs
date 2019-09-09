// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    /// <summary>
    /// Settings value required by WeChat.
    /// </summary>
    public class WeChatSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether upload media as temporary media.
        /// </summary>
        /// <value>
        /// If upload all media as temporary media, will expired in 3 days.
        /// </value>
        public bool UploadTemporaryMedia { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether response WeChat request passively.
        /// </summary>
        /// <value>
        /// Response WeChat request in passive response mode.
        /// </value>
        public bool PassiveResponseMode { get; set; }

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

        /// <summary>
        /// Gets or Sets AppSecret.
        /// </summary>
        /// <value>
        /// WeChat app secret.
        /// </value>
        public string AppSecret { get; set; }
    }
}
