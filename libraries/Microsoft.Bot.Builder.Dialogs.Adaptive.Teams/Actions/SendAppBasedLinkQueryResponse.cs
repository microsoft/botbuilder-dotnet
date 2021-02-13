// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Teams.Actions
{
    /// <summary>
    /// Send a messaging extension 'result' response when a Teams Invoke Activity is received with activity.name='composeExtension/queryLink'.
    /// </summary>
    public class SendAppBasedLinkQueryResponse : BaseTeamsCacheInfoResponseDialog
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Teams.SendAppBasedLinkQueryResponse";

        /// <summary>
        /// Initializes a new instance of the <see cref="SendAppBasedLinkQueryResponse"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public SendAppBasedLinkQueryResponse([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets template for the attachment template of a Thumbnail or Hero Card to send.
        /// </summary>
        /// <value>
        /// Template for the card.
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

            if (Card == null)
            {
                throw new ArgumentException($"An activity with attachments is required for {Kind}.");
            }

            Activity activity = await Card.BindAsync(dc, dc.State).ConfigureAwait(false);
            if (activity?.Attachments?.Any() != true)
            {
                throw new InvalidOperationException($"Invalid activity. An attachment is required for {Kind}.");
            }

            var attachments = activity.Attachments.Select(a => new MessagingExtensionAttachment(a.ContentType, null, a.Content));

            var result = new MessagingExtensionResult
            {
                Type = MEResultResponseType.result.ToString(),
                AttachmentLayout = MEAttachmentLayoutResponseType.list.ToString(), // 'list', 'grid'  // TODO: make this configurable
                Attachments = attachments.ToList(),
            };

            var invokeResponse = CreateMessagingExtensionInvokeResponseActivity(dc, result);
            ResourceResponse resourceResponse = await dc.Context.SendActivityAsync(invokeResponse, cancellationToken: cancellationToken).ConfigureAwait(false);
            return await dc.EndDialogAsync(resourceResponse, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override string OnComputeId()
        {
            return $"{GetType().Name}[{Card?.ToString() ?? string.Empty}]";
        }
    }
}
