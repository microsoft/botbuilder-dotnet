// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    public delegate Task<DialogTurnResult> WaterfallStep(DialogContext dc, WaterfallStepContext step);
}
