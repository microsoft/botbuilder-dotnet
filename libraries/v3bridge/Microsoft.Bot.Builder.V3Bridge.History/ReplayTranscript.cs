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
using Microsoft.Bot.Builder.V3Bridge.Dialogs.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.V3Bridge.History
{
    /// <summary>
    /// Class to collect and then replay activities as a transcript.
    /// </summary>
    public sealed class ReplayTranscript
    {
        private IBotToUser _botToUser;
        private Func<IActivity, string> _header;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="botToUser">Where to replay transcript.</param>
        /// <param name="header">Function for defining the transcript header on each message.</param>
        public ReplayTranscript(IBotToUser botToUser, Func<IActivity, string> header = null)
        {
            _botToUser = botToUser;
            _header = header;
            if (_header == null)
            {
                _header = (activity) => $"({activity.From.Name} {activity.Timestamp:g})";
            }
        }

        /// <summary>
        /// Replay activity to IBotToUser.
        /// </summary>
        /// <param name="activity">Activity.</param>
        /// <returns>Task.</returns>
        public async Task Replay(IActivity activity)
        {
            if (activity is IMessageActivity)
            {
                var msg = _botToUser.MakeMessage();
                msg.Text = _header(activity);
                await _botToUser.PostAsync(msg);

                var act = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(activity));
                if (act.ChannelId != msg.ChannelId)
                {
                    act.ChannelData = null;
                }
                act.From = msg.From;
                act.Recipient = msg.Recipient;
                act.ReplyToId = msg.ReplyToId;
                act.ChannelId = msg.ChannelId;
                act.Conversation = msg.Conversation;
                await _botToUser.PostAsync(act);
            }
        }
    }
}
