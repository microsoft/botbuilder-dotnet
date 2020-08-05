// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.DataModels
{
    internal interface ICoercion
    {
        object Coerce(object source, Type target);
    }
}
