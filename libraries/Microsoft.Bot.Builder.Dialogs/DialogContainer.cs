﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.TraceExtensions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// A container for a set of Dialogs.
    /// </summary>
    public abstract class DialogContainer : Dialog
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DialogContainer"/> class.
        /// </summary>
        /// <param name="dialogId">The ID to assign to the dialog.</param>
        protected DialogContainer(string dialogId = null)
            : this(false, dialogId)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogContainer"/> class.
        /// </summary>
        /// <param name="dialogId">The ID to assign to the dialog.</param>
        /// <param name="ignoreMemoryScopeBinding">False if this <see cref="DialogContainer"/> should interact with MemoryScopes.</param>
        protected DialogContainer(bool ignoreMemoryScopeBinding, string dialogId = null)
            : base(dialogId)
        {
            IgnoreMemoryScopeBinding = ignoreMemoryScopeBinding;
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not this <see cref="DialogContainer"/> should interact with Memory Scopes.
        /// </summary>
        /// <value>The setting for whether or not this <see cref="DialogContainer"/> should ignore scope binding.</value>
        [JsonProperty("ignoreMemoryScopeBinding")]
        public bool IgnoreMemoryScopeBinding { get; set; } = false;

        /// <summary>
        /// Gets or sets the containers <see cref="DialogSet"/>.
        /// </summary>
        /// <value>The containers Dialog Set.</value>
        [JsonProperty("dialogs")]
        public DialogSet Dialogs { get; set; } = new DialogSet();

        /// <summary>
        /// Gets or sets the <see cref="IBotTelemetryClient"/> to use for logging.
        /// When setting this property, all of the contained dialogs' <see cref="Dialog.TelemetryClient"/>
        /// properties are also set.
        /// </summary>
        /// <value>The <see cref="IBotTelemetryClient"/> to use when logging.</value>
        /// <seealso cref="DialogSet.TelemetryClient"/>
        [JsonIgnore]
        public override IBotTelemetryClient TelemetryClient
        {
            get
            {
                return base.TelemetryClient;
            }

            set
            {
                base.TelemetryClient = value ?? NullBotTelemetryClient.Instance;

                // Care! Dialogs.TelemetryClient assigment internally assigns the 
                // TelemetryClient for each dialog which could lead to an eventual stack
                // overflow in cyclical dialg structures. 
                // Don't set the telemetry client if the candidate instance is the same as the currently set one.
                if (Dialogs.TelemetryClient != value)
                {
                    Dialogs.TelemetryClient = base.TelemetryClient;
                }
            }
        }

        /// <summary>
        /// Creates an inner dialog context for the containers active child.
        /// </summary>
        /// <param name="dc">Parents dialog context.</param>
        /// <returns>A new dialog context for the active child.</returns>
        public abstract DialogContext CreateChildContext(DialogContext dc);

        /// <summary>
        /// Finds a child dialog that was previously added to the container.
        /// </summary>
        /// <param name="dialogId">The ID of the dialog to lookup.</param>
        /// <returns>The Dialog if found; otherwise null.</returns>
        public virtual Dialog FindDialog(string dialogId)
        {
            return this.Dialogs.Find(dialogId);
        }

        /// <summary>
        /// Finds a child dialog that was previously added to the container. Uses DialogContext as fallback to gather the dialog.
        /// </summary>
        /// <param name="dialogId">The ID of the dialog to lookup.</param>
        /// <param name="dc">The dialog context fallback where to find the dialog.</param>
        /// <returns>The Dialog if found; otherwise null.</returns>
        public virtual Dialog FindDialog(string dialogId, DialogContext dc = null)
        {
            return Dialogs.Find(dialogId) ?? dc?.Dialogs?.Find(dialogId);
        }

        /// <summary>
        /// Called when an event has been raised, using `DialogContext.emitEvent()`, by either the current dialog or a dialog that the current dialog started.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of conversation.</param>
        /// <param name="e">The event being raised.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if the event is handled by the current dialog and bubbling should stop.</returns>
        public override async Task<bool> OnDialogEventAsync(DialogContext dc, DialogEvent e, CancellationToken cancellationToken)
        {
            var handled = await base.OnDialogEventAsync(dc, e, cancellationToken).ConfigureAwait(false);

            // Trace unhandled "versionChanged" events.
            if (!handled && e.Name == DialogEvents.VersionChanged)
            {
                var traceMessage = $"Unhandled dialog event: {e.Name}. Active Dialog: {dc.ActiveDialog.Id}";

                dc.Dialogs.TelemetryClient.TrackTrace(traceMessage, Severity.Warning, null);

                await dc.Context.TraceActivityAsync(traceMessage, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }

            return handled;
        }

        /// <summary>
        /// GetInternalVersion - Returns internal version identifier for this container.
        /// </summary>
        /// <remarks>
        /// DialogContainers detect changes of all sub-components in the container and map that to an DialogChanged event.
        /// Because they do this, DialogContainers "hide" the internal changes and just have the .id. This isolates changes
        /// to the container level unless a container doesn't handle it.  To support this DialogContainers define a
        /// protected virtual method GetInternalVersion() which computes if this dialog or child dialogs have changed
        /// which is then examined via calls to CheckForVersionChangeAsync().
        /// </remarks>
        /// <returns>version which represents the change of the internals of this container.</returns>
        protected virtual string GetInternalVersion()
        {
            return this.Dialogs.GetVersion();
        }

        /// <summary>
        /// CheckForVersionChangeAsync.
        /// </summary>
        /// <param name="dc">dialog context.</param>
        /// <param name="cancellationToken">cancellationToken.</param>
        /// <returns>task.</returns>
        /// <remarks>
        /// Checks to see if a containers child dialogs have changed since the current dialog instance
        /// was started.
        /// 
        /// This should be called at the start of `beginDialog()`, `continueDialog()`, and `resumeDialog()`.
        /// </remarks>
        protected virtual async Task CheckForVersionChangeAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            var current = dc.ActiveDialog.Version;
            dc.ActiveDialog.Version = this.GetInternalVersion();

            // Check for change of previously stored hash
            if (current != null && current != dc.ActiveDialog.Version)
            {
                // Give bot an opportunity to handle the change.
                // - If bot handles it the changeHash will have been updated as to avoid triggering the 
                //   change again.
                await dc.EmitEventAsync(DialogEvents.VersionChanged, this.Id, true, false, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
