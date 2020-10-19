// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Send a messaging extension 'message' response.
    /// </summary>
    public class SendMessagingExtensionSelectItemResponse : Dialog
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Teams.SendMessagingExtensionSelectItemResponse";

        /// <summary>
        /// Initializes a new instance of the <see cref="SendMessagingExtensionSelectItemResponse"/> class.
        /// </summary>
        /// <param name="activity">Activity containing an attachment to use to create response.</param>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public SendMessagingExtensionSelectItemResponse(Activity activity = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            this.Activity = new StaticActivityTemplate(activity);
        }

        /// <summary>
        /// Gets or sets an optional expression which if is true will disable this action.
        /// </summary>
        /// <example>
        /// "user.age > 18".
        /// </example>
        /// <value>
        /// A boolean expression. 
        /// </value>
        [JsonProperty("disabled")]
        public BoolExpression Disabled { get; set; }

        /// <summary>
        /// Gets or sets template for the activity expression containing a Hero Card or Adaptive Card with an Attachment to send.
        /// </summary>
        /// <value>
        /// Template for the activity.
        /// </value>
        [JsonProperty("activity")]
        public ITemplate<Activity> Activity { get; set; }

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

            if (this.Disabled != null && this.Disabled.GetValue(dc.State) == true)
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            var boundActivity = await Activity.BindAsync(dc, dc.State).ConfigureAwait(false);

            if (boundActivity.Attachments == null || !boundActivity.Attachments.Any())
            {
                throw new ArgumentException($"Invalid Activity. A valid attachment is required for {SendMessagingExtensionSelectItemResponse.Kind}.");
            }

            var attachment = boundActivity.Attachments[0];

            var properties = new Dictionary<string, string>()
            {
                { "SendMessagingExtensionSelectItemResponse", attachment.ToString() },
            };
            TelemetryClient.TrackEvent("GeneratorResult", properties);

            var extentionAttachment = new MessagingExtensionAttachment(attachment.ContentType, null, attachment);

            var activity = new Activity
            {
                Value = new InvokeResponse
                {
                    Status = (int)HttpStatusCode.OK,
                    Body = new Schema.Teams.MessagingExtensionResponse
                    {
                        ComposeExtension = new MessagingExtensionResult
                        {
                            Type = "result",
                            AttachmentLayout = "list", // TODO: enum this
                            Attachments = new List<MessagingExtensionAttachment> { extentionAttachment }
                        }
                    }
                }, 
                Type = ActivityTypesEx.InvokeResponse 
            };

            ResourceResponse sendResponse = await dc.Context.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);

            return await dc.EndDialogAsync(sendResponse, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the compute Id for the dialog.
        /// </summary>
        /// <returns>A string representing the compute Id.</returns>
        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}[{this.Activity?.ToString() ?? string.Empty}]";
        }
    }
}
