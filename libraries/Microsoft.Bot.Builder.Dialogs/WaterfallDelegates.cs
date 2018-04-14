// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    public delegate Task SkipStepFunction(object args = null);

    public delegate Task WaterfallStep(DialogContext dc, object args = null, SkipStepFunction next = null);
}
