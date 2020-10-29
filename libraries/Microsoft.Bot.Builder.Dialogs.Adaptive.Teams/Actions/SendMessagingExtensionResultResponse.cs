// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Send a messaging extension 'result' response.
    /// </summary>
    public class SendMessagingExtensionResultResponse : BaseTeamsCacheInfoResponseDialog
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Teams.SendMessagingExtensionResultResponse";

        /// <summary>
        /// Initializes a new instance of the <see cref="SendMessagingExtensionResultResponse"/> class.
        /// </summary>
        /// <param name="attachmentLayout">Layout for the attachments in the response. 'list' or 'grid'.</param>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public SendMessagingExtensionResultResponse(string attachmentLayout = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            this.AttachmentLayout = attachmentLayout ?? "list";
        }

        /// <summary>
        /// Gets or sets response message to send.
        /// </summary>
        /// <value>
        /// Message to send.
        /// </value>
        [JsonProperty("message")]
        public StringExpression AttachmentLayout { get; set; } = "list";

        /*
                 /// <summary>
                /// Gets or sets the ResponseType.
                /// </summary>
                /// <value>
                /// The ResponseType.
                /// </value>
                [JsonProperty("responseType")]
                public EnumExpression<ResponseTypes> ResponseType { get; set; } = ResponseTypes.Json;
         */

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

            string layout = AttachmentLayout.GetValue(dc.State) ?? "list";

            var properties = new Dictionary<string, string>()
            {
                { "SendMessagingExtensionResultResponse", layout },
            };
            TelemetryClient.TrackEvent("GeneratorResult", properties);

            var response = new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result", //'result', 'auth', 'config', 'message', 'botMessagePreview'
                    AttachmentLayout = layout, // 'list', 'grid'
                    Attachments = null, //new List<MessagingExtensionAttachment> { attachment }
                },
                CacheInfo = GetCacheInfo(dc),
            };

            var invokeResponse = CreateInvokeResponseActivity(response);
            ResourceResponse sendResponse = await dc.Context.SendActivityAsync(invokeResponse, cancellationToken: cancellationToken).ConfigureAwait(false);

            return await dc.EndDialogAsync(sendResponse, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the compute Id for the dialog.
        /// </summary>
        /// <returns>A string representing the compute Id.</returns>
        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}[{this.AttachmentLayout?.ToString() ?? string.Empty}]";
        }
    }
}
