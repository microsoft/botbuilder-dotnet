// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Teams.Actions;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Teams.Actions
{
    /// <summary>
    /// Send a messaging extension 'message' response.
    /// </summary>
    public class SendMEMessageResponse : BaseTeamsCacheInfoResponseDialog
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Teams.SendMEMessageResponse";

        /// <summary>
        /// Initializes a new instance of the <see cref="SendMEMessageResponse"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public SendMEMessageResponse([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets the template or text to use to generate the response message to send.
        /// </summary>
        /// <value>
        /// Message to send.
        /// </value>
        [JsonProperty("message")]
        public StringExpression Message { get; set; }

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

            string message = Message.GetValueOrNull(dc.State);
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentException($"A {nameof(Message)} is required for {Kind}.");
            }

            var response = new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = MEResultResponseType.message.ToString(),
                    Text = message,
                },
                CacheInfo = GetCacheInfo(dc)
            };

            var invokeResponse = CreateInvokeResponseActivity(response);
            ResourceResponse sendResponse = await dc.Context.SendActivityAsync(invokeResponse, cancellationToken: cancellationToken).ConfigureAwait(false);

            return await dc.EndDialogAsync(sendResponse, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override string OnComputeId()
        {
            return $"{GetType().Name}[{Message?.ToString() ?? string.Empty}]";
        }
    }
}
