// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.BotKit.Core
{
    /// <summary>
    /// Interface for implementing a BotkitHandler.
    /// </summary>
    public interface IBotkitHandler
    {
        /// <summary>
        /// Instanciates a BotkitHandler.
        /// </summary>
        /// <param name="botWorker">An instance of BotWorker class.</param>
        /// <param name="botkitMessage">A botkit message.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<object> Handler(BotWorker botWorker, IBotkitMessage botkitMessage);
    }
}
