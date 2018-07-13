// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Represents a request to update a previous message activity in a conversation.
    /// </summary>
    public class MessageUpdateActivity : MessageActivity
    {
        public MessageUpdateActivity() : base(ActivityTypes.MessageUpdate)
        {
        }
    }
}
