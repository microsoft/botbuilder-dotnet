// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Responses
{
    public class MessageMenuResponse : ResponseMessage
    {
        public MessageMenu MessageMenu { get; set; }

        public override string MsgType => ResponseMessageTypes.MessageMenu;
    }
}
