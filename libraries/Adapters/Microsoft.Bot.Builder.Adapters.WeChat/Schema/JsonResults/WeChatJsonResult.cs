// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Globalization;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.JsonResults
{
    public class WeChatJsonResult
    {
        /// <summary>
        /// Gets or sets ErrorCode.
        /// </summary>
        /// <value>
        /// Error code from WeChat, default is 0.
        /// </value>
        [JsonProperty("errcode")]
        public int ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets ErrorMessage.
        /// </summary>
        /// <value>
        /// Error message from WeChat, default is 'ok'.
        /// </value>
        [JsonProperty("errmsg")]
        public virtual string ErrorMessage { get; set; }

        public override string ToString() => string.Format(CultureInfo.InvariantCulture, "WeChatJsonResult: {{ErrorCode:'{0}', ErrorMessage:'{1}'}}", ErrorCode, ErrorMessage);
    }
}
