// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Runtime.Tests.Components.TestComponents
{
    public class SendActivityAsPirate : Dialog
    {
        public SendActivityAsPirate(string data = null)
        {
            Data = data;
        }

        public static string Kind { get; internal set; } = "Test.SendActivityAsPirate";

        public string Data { get; private set; }

        public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
