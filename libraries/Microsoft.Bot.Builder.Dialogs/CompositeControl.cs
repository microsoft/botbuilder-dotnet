// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class CompositeControl : Dialog
    {
        protected DialogSet Dialogs { get; set; }
        protected string DialogId { get; set; }
        protected object DefaultOptions { get; set; }

        public CompositeControl(DialogSet dialogs, string dialogId, object defaultOptions)
        {
            Dialogs = dialogs;
            DialogId = dialogId;
            DefaultOptions = defaultOptions;
        }

        public override bool HasDialogContinue => true;

        public override bool HasDialogResume => false;

        public async Task<DialogResult> Begin(TurnContext context, object state, object options)
        {
            var cdc = Dialogs.CreateContext(context, state);
            await cdc.Begin(DialogId, options);
            return cdc.DialogResult;
        }

        public async Task<DialogResult> Continue(TurnContext context, object state)
        {
            var cdc = Dialogs.CreateContext(context, state);
            await cdc.Continue();
            return cdc.DialogResult;
        }

        public override async Task DialogBegin(DialogContext dc, object dialogArgs = null)
        {
            // Start the controls entry point dialog. 
            var cdc = Dialogs.CreateContext(dc.Context, dc.Instance.State);
            await cdc.Begin(DialogId, dialogArgs);
            // End if the controls dialog ends.
            if (!cdc.DialogResult.Active)
            {
                await dc.End(cdc.DialogResult.Result);
            }
        }

        public override async Task DialogContinue(DialogContext dc)
        {
            // Continue controls dialog stack.
            var cdc = Dialogs.CreateContext(dc.Context, dc.Instance.State);
            await cdc.Continue();
            // End if the controls dialog ends.
            if (!cdc.DialogResult.Active)
            {
                await dc.End(cdc.DialogResult.Result);
            }
        }
    }
}
