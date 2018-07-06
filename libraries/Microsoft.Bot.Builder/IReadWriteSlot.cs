// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public interface IReadWriteSlot<T> : IReadOnlySlot<T>
    {
        ISlotDefinition Definition { get; }

        IFrame Frame { get;  }

        IReadOnlySlot<T> AsReadOnly();

        Task DeleteAsync(TurnContext context);

        Task SetAsync(TurnContext context, T value);
    }
}
