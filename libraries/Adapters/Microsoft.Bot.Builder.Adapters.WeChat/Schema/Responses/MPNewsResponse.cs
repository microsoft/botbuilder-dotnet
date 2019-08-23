// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Responses
{
    public class MPNewsResponse : ResponseMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MPNewsResponse"/> class.
        /// </summary>
        public MPNewsResponse()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MPNewsResponse"/> class.
        /// </summary>
        /// <param name="senderId">The sender's id.</param>
        /// <param name="recipientId">The recipient id.</param>
        /// <param name="mediaId">The media id of the mpnews.</param>
        public MPNewsResponse(string senderId, string recipientId, string mediaId)
            : base(senderId, recipientId)
        {
            MediaId = mediaId;
        }

        public override string MsgType => ResponseMessageTypes.MPNews;

        public string MediaId { get; set; }
    }
}
