// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Base class for controls
    /// </summary>
    public abstract class Control
    {
        private IDialogOptions _defaultOptions;

        /// <summary>
        /// Creates a new Control instance.
        /// </summary>
        public Control(IDialogOptions defaultOptions = null)
        {
            _defaultOptions = defaultOptions;
        }

        public async Task<DialogResult> Begin(TurnContext context, object state, IDialogOptions options)
        {
            BotAssert.ContextNotNull(context);
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            // Create empty dialog set and ourselves to it
            var dialogs = new DialogSet();
            dialogs.Add("control", (IDialog)this);

            // Start the control
            var cdc = dialogs.CreateContext(context, state);
            await cdc.Begin("control", options.ApplyDefaults(_defaultOptions));
            return cdc.DialogResult;
        }
        public async Task<DialogResult> Continue(TurnContext context, object state)
        {
            BotAssert.ContextNotNull(context);
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            // Create empty dialog set and ourselves to it
            var dialogs = new DialogSet();
            dialogs.Add("control", (IDialog)this);

            // Start the control
            var cdc = dialogs.CreateContext(context, state);
            await cdc.Continue();
            return cdc.DialogResult;
        }
    }
}
