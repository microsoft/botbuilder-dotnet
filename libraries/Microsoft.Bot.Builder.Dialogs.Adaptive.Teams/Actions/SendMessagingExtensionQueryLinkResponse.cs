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
    /// Send a messaging extension 'result' response when a Teams Invoke Activity is received with activity.name='composeExtension/queryLink'.
    /// </summary>
    public class SendMessagingExtensionQueryLinkResponse : Dialog
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Teams.SendMessagingExtensionQueryLinkResponse";

        /// <summary>
        /// Initializes a new instance of the <see cref="SendMessagingExtensionQueryLinkResponse"/> class.
        /// </summary>
        /// <param name="activity">Activity containing an Attachment Thumbnail or Hero Card to send.</param>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public SendMessagingExtensionQueryLinkResponse(Activity activity, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            this.Activity = new StaticActivityTemplate(activity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendMessagingExtensionQueryLinkResponse"/> class.
        /// </summary>
        /// <param name="text">Optional, template to evaluate to create the activity.</param>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public SendMessagingExtensionQueryLinkResponse(string text = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            this.Activity = new ActivityTemplate(text ?? string.Empty);
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
        /// Gets or sets template for the attachment template of a Thumbnail or Hero Card to send.
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

            Attachment attachment = null;
            if (Activity != null)
            {
                var boundActivity = await Activity.BindAsync(dc, dc.State).ConfigureAwait(false);

                if (boundActivity.Attachments == null || !boundActivity.Attachments.Any())
                {
                    throw new ArgumentException($"Invalid Activity. A attachment is required for Send Messaging Extension Query Link Response.");
                }

                attachment = boundActivity.Attachments[0];
            }

            var properties = new Dictionary<string, string>()
            {
                { "SendMessagingExtensionQueryLinkResponse", attachment.ToString() },
            };
            TelemetryClient.TrackEvent("GeneratorResult", properties);

            var extentionAttachment = new MessagingExtensionAttachment(attachment.ContentType, null, attachment);
            
            var activity = new Activity
            {
                Value = new InvokeResponse
                {
                    Status = (int)HttpStatusCode.OK,
                    Body = new MessagingExtensionResponse
                    {
                        ComposeExtension = new MessagingExtensionResult
                        {
                            Type = "result", //'result', 'auth', 'config', 'message', 'botMessagePreview'
                            AttachmentLayout = "list", // 'list', 'grid'  // TODO: make this configurable
                            Attachments = new[] { extentionAttachment },
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
