// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK GitHub:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.V3Bridge.History
{
    /// <summary>
    /// Interface for getting activities from some source.
    /// </summary>
    public interface IActivitySource
    {
        /// <summary>
        /// Produce an enumeration over conversation.
        /// </summary>
        /// <param name="channelId">Channel where conversation happened.</param>
        /// <param name="conversationId">Conversation within the channel.</param>
        /// <param name="oldest">Earliest time to include.</param>
        /// <returns>Enumeration over the recorded activities.</returns>
        /// <remarks>Activities are ordered by channel, then conversation, then time ascending.</remarks>
        IEnumerable<IActivity> Activities(string channelId, string conversationId, DateTime oldest = default(DateTime));

        /// <summary>
        /// Walk over recorded activities and call a function on them.
        /// </summary>
        /// <param name="function">Function to apply to each actitivty.</param>
        /// <param name="channelId">ChannelId to filter on or null for no filter.</param>
        /// <param name="conversationId">ConversationId to filter on or null for no filter.</param>
        /// <param name="oldest">Oldest timestamp to include.</param>
        /// <param name="cancel">Cancellation token.</param>
        /// <returns></returns>
        /// <remarks>Activities are ordered by channel, then conversation, then time ascending.</remarks>
        Task WalkActivitiesAsync(Func<IActivity, Task> function, string channelId = null, string conversationId = null, DateTime oldest = default(DateTime), CancellationToken cancel = default(CancellationToken));
    }
}
