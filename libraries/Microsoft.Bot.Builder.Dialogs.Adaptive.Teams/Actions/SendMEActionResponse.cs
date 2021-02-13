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
    public class SendMEActionResponse : BaseSendTaskModuleContinueResponse
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Teams.SendMEActionResponse";

        /// <summary>
        /// Initializes a new instance of the <see cref="SendMEActionResponse"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public SendMEActionResponse([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets template for the activity expression containing a Card to send.
        /// </summary>
        /// <value>
        /// Template for the activity containing an Adaptive Card to send.
        /// </value>
        [JsonProperty("card")]
        public ITemplate<Activity> Card { get; set; }

        /// <inheritdoc/>
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            if (Disabled != null && Disabled.GetValue(dc.State))
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

        /// <inheritdoc/>
        protected override string OnComputeId()
        {
            return $"{GetType().Name}[{Card?.ToString() ?? string.Empty}]";
        }
    }
}
