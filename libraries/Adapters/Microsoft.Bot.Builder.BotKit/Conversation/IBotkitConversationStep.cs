// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.BotKit.Conversation
{
    /// <summary>
    /// Interface for BotKitConversation.
    /// </summary>
    public interface IBotkitConversationStep
    {
        /// <summary>
        /// Gets or sets the number pointing to the current message in the current thread in this dialog's script.
        /// </summary>
        /// <value>The index to the current message.</value>
        int Index { get; set; }

        /// <summary>
        /// Gets or sets the name of the current thread.
        /// </summary>
        /// <value>The current thread.</value>
        string Thread { get; set; }

        /// <summary>
        /// Gets or sets a pointer to the current dialog state.
        /// </summary>
        /// <value>The current DialogState.</value>
        object State { get; set; }

        /// <summary>
        /// Gets or sets a pointer to any options passed into the dialog when it began.
        /// </summary>
        /// <value>The options passed to the Dialog.</value>
        object Options { get; set; }

        /// <summary>
        /// Gets or sets the reason for this step being called.
        /// </summary>
        /// <value>The reason for this step being called.</value>
        DialogReason Reason { get; set; }

        /// <summary>
        /// Gets or sets the results of the previous turn.
        /// </summary>
        /// <value>The result of the turn.</value>
        object Result { get; set; }

        /// <summary>
        /// Gets or sets a pointer directly to state.values.
        /// </summary>
        /// <value>The State.values.</value>
        object Values { get; set; }

        /// <summary>
        /// A function to call when the step is completed.
        /// </summary>
        /// <param name="stepresult">Step result.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task Next(object stepresult);
    }
}
