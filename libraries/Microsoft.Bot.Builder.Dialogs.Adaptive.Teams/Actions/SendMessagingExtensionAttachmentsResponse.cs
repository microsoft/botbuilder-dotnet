// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Teams.Actions
{
    /// <summary>
    /// Send a messaging extension 'result' in response to a Teams Invoke with name of 'composeExtension/query'.
    /// </summary>
    public class SendMessagingExtensionAttachmentsResponse : BaseTeamsCacheInfoResponseDialog
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Teams.SendMessagingExtensionAttachmentsResponse";

        /// <summary>
        /// Initializes a new instance of the <see cref="SendMessagingExtensionAttachmentsResponse"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public SendMessagingExtensionAttachmentsResponse([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets the Activity containing the Attachments to send.
        /// </summary>
        /// <value>
        /// Activity with the Attachments to send in response to the Query.
        /// </value>
        [JsonProperty("attachments")]
        public ITemplate<Activity> Attachments { get; set; }

        /// <summary>
        /// Gets or sets the Attachment Layout type for the response ('grid' or 'list').
        /// </summary>
        /// <value>
        /// The Attachment Layout type.
        /// </value>
        [JsonProperty("attachmentLayout")]
        public EnumExpression<MessagingExtensionAttachmentLayoutResponseType> AttachmentLayout { get; set; } = MessagingExtensionAttachmentLayoutResponseType.list;

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
            if (Attachments != null)
            {
                activity = await Attachments.BindAsync(dc, dc.State).ConfigureAwait(false);
            }

            if (activity?.Attachments?.Any() != true)
            {
                throw new InvalidOperationException($"Missing attachments in {Kind}.");
            }

            var layout = AttachmentLayout.GetValue(dc.State);
            var attachments = activity.Attachments.Select(a => new MessagingExtensionAttachment(a.ContentType, null, a.Content));

            var result = new MessagingExtensionResult
            {
                Type = MessagingExtensionResultResponseType.result.ToString(),
                AttachmentLayout = layout.ToString(),
                Attachments = attachments.ToList(),
            };

            var invokeResponse = CreateMessagingExtensionInvokeResponseActivity(dc, result);
            ResourceResponse resourceResponse = await dc.Context.SendActivityAsync(invokeResponse, cancellationToken: cancellationToken).ConfigureAwait(false);
            return await dc.EndDialogAsync(resourceResponse, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the compute Id for the dialog.
        /// </summary>
        /// <returns>A string representing the compute Id.</returns>
        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}[{this.Attachments?.ToString() ?? string.Empty}]";
        }
    }
}
