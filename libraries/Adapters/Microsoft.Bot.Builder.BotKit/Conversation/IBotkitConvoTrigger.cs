// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System.Text.RegularExpressions;

namespace Microsoft.Bot.Builder.BotKit.Conversation
{
    /// <summary>
    /// Definition of the trigger pattern passed into .ask or .addQuestion.
    /// </summary>
    public interface IBotkitConvoTrigger
    {
        /// <summary>
        /// BotkitConvoTrigger.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <param name="type">The type.</param>
        /// <param name="pattern">The pattern.</param>
        /// <param name="defaultTrigger">The default trigger.</param>
        void BotkitConvoTrigger(IBotkitConvoHandler handler, string type = null, string pattern = null, bool defaultTrigger = false);

        /// <summary>
        /// BotkitConvoTrigger.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <param name="type">The type.</param>
        /// <param name="pattern">The pattern.</param>
        /// <param name="defaultTrigger">The default trigger.</param>
        void BotkitConvoTrigger(IBotkitConvoHandler handler, string type = null, Regex pattern = null, bool defaultTrigger = false);
    }
}
