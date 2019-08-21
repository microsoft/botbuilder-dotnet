// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs
{
    public interface IDialogDependencies
    {
        List<IDialog> ListDependencies();
    }
}
