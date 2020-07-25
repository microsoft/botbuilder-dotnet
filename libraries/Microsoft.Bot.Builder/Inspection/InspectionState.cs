// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// A state management object that automates the reading and writing of the associated 
    /// state properties to a storage layer. Used by Inspection middleware that enables
    /// debugging bot state.
    /// </summary>
    public class InspectionState : BotState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InspectionState"/> class.
        /// </summary>
        /// <param name="storage">The storage layer this state management object will use to store
        /// and retrieve state.</param>
        public InspectionState(IStorage storage)
            : base(storage, nameof(InspectionState))
        {
        }

        /// <summary>
        /// Gets the key to use when reading and writing state to and from storage.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <returns>The storage key.</returns>
        protected override string GetStorageKey(ITurnContext turnContext)
        {
            // This is shared state across all bots that use this underlying IStorage
            return nameof(InspectionState);
        }
    }
}
