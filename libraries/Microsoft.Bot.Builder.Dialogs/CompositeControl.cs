// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class CompositeControl : IDialogContinue
    {
        protected DialogSet Dialogs { get; set; }
        protected string DialogId { get; set; }
        protected object DefaultOptions { get; set; }

        public CompositeControl(DialogSet dialogs, string dialogId, object defaultOptions)
        {
            if (string.IsNullOrEmpty(dialogId))
                throw new ArgumentNullException(nameof(dialogId));

            Dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
            DialogId = dialogId;
            DefaultOptions = defaultOptions ?? throw new ArgumentNullException(nameof(defaultOptions));
        }

        public async Task<DialogResult> Begin(TurnContext context, object state, object options)
        {
            BotAssert.ContextNotNull(context);
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var cdc = Dialogs.CreateContext(context, state);
            await cdc.Begin(DialogId, options);
            return cdc.DialogResult;
        }

        public async Task<DialogResult> Continue(TurnContext context, object state)
        {
            BotAssert.ContextNotNull(context);
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            var cdc = Dialogs.CreateContext(context, state);
            await cdc.Continue();
            return cdc.DialogResult;
        }

        public async Task DialogBegin(DialogContext dc, object dialogArgs = null)
        {
            if (dc == null)
                throw new ArgumentNullException(nameof(dc));

            // Start the controls entry point dialog. 
            var cdc = Dialogs.CreateContext(dc.Context, dc.Instance.State);
            await cdc.Begin(DialogId, dialogArgs);
            // End if the controls dialog ends.
            if (!cdc.DialogResult.Active)
            {
                await dc.End(cdc.DialogResult.Result);
            }
        }

        public async Task DialogContinue(DialogContext dc)
        {
            if (dc == null)
                throw new ArgumentNullException(nameof(dc));

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
