// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder
{
    public interface ITurnContext<T> : ITurnContext
    {
        new T Activity { get; }
    }
}
