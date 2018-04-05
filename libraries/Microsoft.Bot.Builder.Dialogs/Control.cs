// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Base class for controls
    /// </summary>
    public abstract class Control : Dialog
    {
        private DialogOptions _defaultOptions;

        /// <summary>
        /// Creates a new Control instance.
        /// </summary>
        /// <param name="defaultOptions">(Optional) set of default options that should be passed to controls root dialog. These will be merged with arguments passed in by the caller.</param>
        public Control(DialogOptions defaultOptions = null)
        {
            _defaultOptions = defaultOptions;
        }

        public async Task<DialogResult> Begin(TurnContext context, object state, DialogOptions options)
        {
            // Create empty dialog set and ourselves to it
            var dialogs = new DialogSet();
            dialogs.Add("control", this);

            // Start the control
            var cdc = dialogs.CreateContext(context, state);
            await cdc.Begin("control", options);
            return cdc.DialogResult;
        }
        public async Task<DialogResult> Continue(TurnContext context, object state)
        {
            // Create empty dialog set and ourselves to it
            var dialogs = new DialogSet();
            dialogs.Add("control", this);

            // Start the control
            var cdc = dialogs.CreateContext(context, state);
            await cdc.Continue();
            return cdc.DialogResult;
        }
    }
}
