// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    public class MyWaterfallDialog : WaterfallDialog
    {
        public MyWaterfallDialog(string id)
            : base(id)
        {
            AddStep(Waterfall2_Step1);
            AddStep(Waterfall2_Step2);
            AddStep(Waterfall2_Step3);
        }

        private static async Task<DialogTurnResult> Waterfall2_Step1(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("step1");
            return Dialog.EndOfTurn;
        }

        private static async Task<DialogTurnResult> Waterfall2_Step2(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("step2");
            return Dialog.EndOfTurn;
        }

        private static async Task<DialogTurnResult> Waterfall2_Step3(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("step3");
            return Dialog.EndOfTurn;
        }
    }
}
