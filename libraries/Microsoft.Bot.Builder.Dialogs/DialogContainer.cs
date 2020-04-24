// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs
{
    public abstract class DialogContainer : Dialog
    {
        protected DialogContainer(string dialogId = null)
            : base(dialogId)
        {
        }

        [JsonIgnore]
        public DialogSet Dialogs { get; set; } = new DialogSet();

        public abstract DialogContext CreateChildContext(DialogContext dc);

        public virtual Dialog FindDialog(string dialogId)
        {
            return this.Dialogs.Find(dialogId);
        }

        public override string GetVersion()
        {
            // use dialogset's concept of version.
            return this.Dialogs.GetVersion();
        }

        /// <summary>
        /// CheckForVersionChangeAsync.
        /// </summary>
        /// <param name="dc">dialog context.</param>
        /// <returns>task.</returns>
        /// <remarks>
        /// Checks to see if a containers child dialogs have changed since the current dialog instance
        /// was started.
        /// 
        /// This should be called at the start of `beginDialog()`, `continueDialog()`, and `resumeDialog()`.
        /// </remarks>
        protected async Task CheckForVersionChangeAsync(DialogContext dc)
        {
            var current = dc.ActiveDialog.Version;
            dc.ActiveDialog.Version = this.GetVersion();

            // Check for change of previously stored hash
            if (current != dc.ActiveDialog.Version)
            {
                // Give bot an opportunity to handle the change.
                // - If bot handles it the changeHash will have been updated as to avoid triggering the 
                //   change again.
                var handled = await dc.EmitEventAsync(DialogEvents.DialogChanged, this.Id, true, false).ConfigureAwait(false);
                if (!handled)
                {
                    // Throw an error for bot to catch
                    throw new Exception($"Version change detected for '{this.Id}' dialog.");
                }
            }
        }
    }
}
