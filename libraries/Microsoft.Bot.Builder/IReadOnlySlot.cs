// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public interface IReadOnlySlot<T>
    {
        Task<T> GetAsync(TurnContext context);

        Task<bool> HasAsync(TurnContext context);

        Task<IEnumerable<SlotHistoryValue<T>>> HistoryAsync(TurnContext context);
    }
}
