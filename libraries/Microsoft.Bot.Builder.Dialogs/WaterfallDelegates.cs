// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    public delegate Task SkipStepFunction(IDictionary<string, object> args = null);

    public delegate Task WaterfallStep(DialogContext dc, IDictionary<string, object> args = null, SkipStepFunction next = null);
}
