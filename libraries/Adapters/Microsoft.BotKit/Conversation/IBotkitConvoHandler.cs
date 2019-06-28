// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.BotKit.Conversation
{
    /// <summary>
    /// Definition of the handler functions used to handle .ask and .addQuestion conditions.
    /// </summary>
    public interface IBotkitConvoHandler
    {
        /// <summary>
        /// Description for ConvoHandler.
        /// </summary>
        /// <param name="answer">The answer.</param>
        /// <param name="convo">The BotkitDialogWrapper.</param>
        /// <param name="bot">The BotWorker bot.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task ConvoHandler(string answer, BotkitDialogWrapper convo, BotWorker bot);
    }
}
