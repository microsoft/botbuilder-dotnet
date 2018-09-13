// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

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

        /// <summary>
        /// Gets the key to use when reading and writing state to and from storage.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <returns>The storage key.</returns>
        protected override string GetStorageKey(ITurnContext turnContext)
        {
            var channelId = turnContext.Activity.ChannelId ?? throw new ArgumentNullException("invalid activity-missing channelId");
            var userId = turnContext.Activity.From?.Id ?? throw new ArgumentNullException("invalid activity-missing From.Id");
            return $"{channelId}/users/{userId}";
        }
    }
}
