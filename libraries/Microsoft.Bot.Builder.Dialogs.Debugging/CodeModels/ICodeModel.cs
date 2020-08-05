// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.CodeModels
{
    internal interface ICodeModel
    {
        string NameFor(object item);

        IReadOnlyList<ICodePoint> PointsFor(DialogContext dialogContext, object item, string more);
    }
}
