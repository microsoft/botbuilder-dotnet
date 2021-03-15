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

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Teams.Actions
{
    /// <summary>
    /// Send a Card Tab response to the user.
    /// </summary>
    public class SendTabCardResponse : Dialog
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Teams.SendTabCardResponse";

        /// <summary>
        /// Initializes a new instance of the <see cref="SendTabCardResponse"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public SendTabCardResponse([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            RegisterSourceLocation(callerPath, callerLine);
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
        /// Gets or sets template for the activity expression containing Adaptive Cards to send.
        /// </summary>
        /// <value>
        /// Template for the cards.
        /// </value>
        [JsonProperty("cards")]
        public ITemplate<Activity> Cards { get; set; }

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

            if (Cards == null)
            {
                throw new ArgumentException($"Valid {nameof(Cards)} are required for {Kind}.");
            }
            
            var activity = await Cards.BindAsync(dc, dc.State).ConfigureAwait(false);
            if (activity?.Attachments?.Any() != true)
            {
                throw new InvalidOperationException($"Invalid activity. Attachment(s) are required for {Kind}.");
            }

            var cards = activity.Attachments.Select(a => new TabResponseCard() { Card = a.Content }).ToArray();
            var responseActivity = GetTabInvokeResponse(cards);

            ResourceResponse sendResponse = await dc.Context.SendActivityAsync(responseActivity, cancellationToken: cancellationToken).ConfigureAwait(false);
            return await dc.EndDialogAsync(sendResponse, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override string OnComputeId()
        {
            if (Cards is ActivityTemplate at)
            {
                return $"{GetType().Name}({StringUtils.Ellipsis(at.Template.Trim(), 30)})";
            }

            return $"{GetType().Name}('{StringUtils.Ellipsis(Cards?.ToString().Trim(), 30)}')";
        }

        private Activity GetTabInvokeResponse(IList<TabResponseCard> cards)
        {
            return new Activity
            {
                Value = new InvokeResponse
                {
                    Status = (int)HttpStatusCode.OK,
                    Body = new TabResponse
                    {
                        Tab = new TabResponsePayload
                        {
                            Type = "continue",
                            Value = new TabResponseCards
                            {
                                Cards = cards
                            }
                        }
                    }
                },
                Type = ActivityTypesEx.InvokeResponse
            };
        }
    }
}
