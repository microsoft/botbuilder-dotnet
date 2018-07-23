// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Handles persistence of a user state object using the user ID as part of the key.
    /// </summary>
    public class UserState : BotState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserState"/> class.
        /// </summary>
        /// <param name="storage">The storage provider to use.</param>
        public UserState(IStorage storage)
            : base(storage, nameof(UserState))
        {
        }

        protected override string GetStorageKey(ITurnContext context) => $"user/{context.Activity.ChannelId}/{context.Activity.From.Id}";
    }
}
