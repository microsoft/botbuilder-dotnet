// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public interface IFrame
    {
        IFrame Parent { get; set; }

        void AddSlot(IReadWriteSlot slot);

        Task LoadAsync(TurnContext context, bool accessed = false);

        Task SlotValueChangedAsync(TurnContext context, string[] tags, object value);
    }
}
