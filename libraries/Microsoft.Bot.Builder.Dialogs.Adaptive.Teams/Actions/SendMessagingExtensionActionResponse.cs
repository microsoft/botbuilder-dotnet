// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Teams.Actions;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Teams.Actions
{
    /// <summary>
    /// Send a messaging extension action response.
    /// </summary>
    public class SendMessagingExtensionActionResponse : BaseSendTaskModuleContinueResponse
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Teams.SendMessagingExtensionActionResponse";

        /// <summary>
        /// Initializes a new instance of the <see cref="SendMessagingExtensionActionResponse"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public SendMessagingExtensionActionResponse([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets template for the activity expression containing a Card to send.
        /// </summary>
        /// <value>
        /// Template for the activity containing an Adaptive Card to send.
        /// </value>
        [JsonProperty("card")]
        public ITemplate<Activity> Card { get; set; }

        /// <summary>
        /// Called when the dialog is started and pushed onto the dialog stack.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="options">Optional, initial information to pass to the dialog.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            if (this.Disabled != null && this.Disabled.GetValue(dc.State))
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            Activity activity = null;
            if (Card != null)
            {
                activity = await Card.BindAsync(dc, dc.State).ConfigureAwait(false);
            }

            if (activity?.Attachments?.Any() != true)
            {
                throw new InvalidOperationException($"Missing attachments in {Kind}.");
            }

            var title = Title.GetValueOrNull(dc.State);
            var height = Height.GetValueOrNull(dc.State);
            var width = Width.GetValueOrNull(dc.State);
            var completionBotId = CompletionBotId.GetValueOrNull(dc.State);

            var response = new MessagingExtensionActionResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo
                    {
                        Card = activity.Attachments[0],
                        Height = height,
                        Width = width,
                        Title = title,
                        CompletionBotId = completionBotId
                    },
                },
            };

            var invokeResponse = CreateInvokeResponseActivity(response);
            var resourceResponse = await dc.Context.SendActivityAsync(invokeResponse, cancellationToken).ConfigureAwait(false);
            return await dc.EndDialogAsync(resourceResponse, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the compute Id for the dialog.
        /// </summary>
        /// <returns>A string representing the compute Id.</returns>
        protected override string OnComputeId()
        {
            return $"{GetType().Name}[{Card?.ToString() ?? string.Empty}]";
        }
    }
}
