// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder
{
    public class InspectionState : BotState
    {
        public InspectionState(IStorage storage)
            : base(storage, nameof(InspectionState))
        {
        }

        protected override string GetStorageKey(ITurnContext turnContext)
        {
            // This is shared state across all bots that use this underlying IStorage
            return nameof(InspectionState);
        }
    }
}
