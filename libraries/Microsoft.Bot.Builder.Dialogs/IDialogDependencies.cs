// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Dialogs
{
    public interface IDialogDependencies
    {
        List<IDialog> ListDependencies();
    }
}
