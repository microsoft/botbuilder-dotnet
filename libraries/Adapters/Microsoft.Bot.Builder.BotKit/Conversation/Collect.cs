// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.BotKit.Conversation
{
    /// <summary>
    /// Collect class.
    /// </summary>
    public class Collect
    {
        /// <summary>
        /// Gets or Sets the Key.
        /// </summary>
        /// <value>The key of the collect.</value>
        public string Key { get; set; }

        /// <summary>
        /// Gets or Sets the Options.
        /// </summary>
        /// <value>The options of the collect.</value>
        public IBotkitConvoTrigger Options { get; set; }
    }
}
