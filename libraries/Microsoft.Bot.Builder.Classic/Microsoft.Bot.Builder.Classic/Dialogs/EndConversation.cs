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

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Classic.Dialogs.Internals;
using Microsoft.Bot.Builder.Classic.Scorables;
using Microsoft.Bot.Builder.Classic.Scorables.Internals;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Classic.Dialogs
{
    /// <summary>
    /// This event represents the end of the conversation.  It is initiated by <see cref="Extensions.EndConversation(IDialogContext, string)"/>
    /// and propagates as an event in the stack scorable process to allow interception.
    /// </summary>
    public sealed class EndConversationEvent
    {
        /// <summary>
        /// Code selected from <see cref="EndOfConversationCodes"/> 
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Construct the <see cref="EndConversationEvent"/>.
        /// </summary>
        /// <param name="code">Code selected from <see cref="EndOfConversationCodes"/>.</param>
        public EndConversationEvent(string code)
        {
            this.Code = code;
        }

        /// <summary>
        /// Construct the scorable used to handle the <see cref="EndConversationEvent"/>.
        /// </summary>
        /// <returns>The constructed scorable.</returns>
        public static IScorable<IResolver, double> MakeScorable()
        {
            var scorable =
                Actions.Bind(async (EndConversationEvent e, IDialogStack stack, IBotToUser botToUser, IBotData data, CancellationToken token) =>
                {
                    stack.Reset();

                    data.ConversationData.Clear();
                    data.PrivateConversationData.Clear();

                    var end = botToUser.MakeMessage();
                    end.Type = ActivityTypes.EndOfConversation;
                    end.AsEndOfConversationActivity().Code = e.Code;

                    await botToUser.PostAsync(end, token);
                })
                .Normalize();

            return scorable;
        }
    }

    public static partial class Extensions
    {
        private static readonly ResumeAfter<IEventActivity> AfterReset = (context, result) => Task.CompletedTask;

        /// <summary>
        /// Initiate an <see cref="EndConversationEvent"/> to reset the conversation's state and stack and send an
        /// <see cref="ActivityTypes.EndOfConversation"/> to the Connector.
        /// </summary>
        public static void EndConversation(this IDialogContext context, string code)
        {
            var activity = new Activity(ActivityTypes.Event) { Value = new EndConversationEvent(code) };
            context.Post(activity, AfterReset);
        }
    }
}
