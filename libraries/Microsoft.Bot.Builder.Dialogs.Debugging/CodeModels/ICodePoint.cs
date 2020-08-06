// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Debugging.CodeModels
{
    internal interface ICodePoint
    {
        object Item { get; }

        string More { get; }

        string Name { get; }

        object Data { get; }

        object Evaluate(string expression);
    }
}
