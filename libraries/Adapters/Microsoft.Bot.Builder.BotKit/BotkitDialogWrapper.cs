// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.BotKit.Conversation;

namespace Microsoft.Bot.Builder.BotKit
{
    /// <summary>
    ///  BotkitDialogWrapper class.
    /// </summary>
    public class BotkitDialogWrapper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BotkitDialogWrapper"/> class.
        /// </summary>
        /// <param name="dialogContext">dialogContext for the BotkitDialogWrapper.</param>
        /// <param name="botkitconvoStep">botkitconvoStep for the BotkitDialogWrapper.</param>
        public BotkitDialogWrapper(DialogContext dialogContext, IBotkitConversationStep botkitconvoStep)
        {
        }

        /// <summary>
        /// Gets or sets an object containing variables and user responses from this conversation.
        /// </summary>
        /// <value>An object with variables.</value>
        public Tuple<string, object> Vars { get; set; }

        /// <summary>
        /// Jump immediately to the first message in a different thread.
        /// </summary>
        /// <param name="thread">Name of a thread.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task GotoThread(string thread)
        {
            await Task.FromException(new NotImplementedException());
        }

        /// <summary>
        ///  Repeat the last message sent on the next turn.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Repeat()
        {
            await Task.FromException(new NotImplementedException());
        }

        /// <summary>
        /// Set the value of a variable that will be available to messages in the conversation.
        /// Equivalent to convo.vars.key = val;
        /// Results in {{vars.key}} being replaced with the value in val.
        /// </summary>
        /// <param name="key">The name of the variable.</param>
        /// <param name="val">The value for the variable.</param>
        public void SetVar(object key, object val)
        {
        }
    }
}
