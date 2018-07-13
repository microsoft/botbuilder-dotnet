// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Conversation is ending, or a request to end the conversation
    /// </summary>
    public class EndOfConversationActivity : MessageActivity
    {
        public EndOfConversationActivity() : base(ActivityTypes.EndOfConversation)
        {
        }

        /// <summary>
        /// Gets or sets code indicating why the conversation has ended.
        /// Possible values include: 'unknown', 'completedSuccessfully',
        /// 'userCancelled', 'botTimedOut', 'botIssuedInvalidMessage',
        /// 'channelFailed'
        /// </summary>
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }
    }
}
