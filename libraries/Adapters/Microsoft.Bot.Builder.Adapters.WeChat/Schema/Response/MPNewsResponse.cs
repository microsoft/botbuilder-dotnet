// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Response
{
    public class MPNewsResponse : ResponseMessage
    {
        public MPNewsResponse(string mediaId)
        {
            MediaId = mediaId;
        }

        public override string MsgType => ResponseMessageType.MPNews;

        public string MediaId { get; set; }
    }
}
