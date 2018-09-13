// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Classic.Dialogs;

namespace Multiple_Dialogs_Bridge.Dialogs
{
    [Serializable]
    public class FlightsDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Flights Dialog is not implemented");
            context.Done(false);
            //context.Fail(new NotImplementedException("Flights Dialog is not implemented and is instead being used to show context.Fail"));
        }
    }
}
