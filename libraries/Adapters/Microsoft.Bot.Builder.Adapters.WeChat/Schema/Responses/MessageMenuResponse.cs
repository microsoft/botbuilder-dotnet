// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Responses
{
    public class MessageMenuResponse : ResponseMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageMenuResponse"/> class.
        /// </summary>
        public MessageMenuResponse()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageMenuResponse"/> class.
        /// </summary>
        /// <param name="senderId">The sender's id.</param>
        /// <param name="recipientId">The recipient id.</param>
        public MessageMenuResponse(string senderId, string recipientId)
            : base(senderId, recipientId)
        {
        }

        public MessageMenu MessageMenu { get; set; }

        public override string MsgType => ResponseMessageTypes.MessageMenu;
    }
}
