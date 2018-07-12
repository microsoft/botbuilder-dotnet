// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public interface IReadWriteSlot : IReadOnlySlot
    {
        SlotDefinition Definition { get; }

        IFrame Frame { get;  }

        IReadOnlySlot AsReadOnly();

        Task DeleteAsync(TurnContext context);

        Task SetAsync(TurnContext context, object value);
    }
}
