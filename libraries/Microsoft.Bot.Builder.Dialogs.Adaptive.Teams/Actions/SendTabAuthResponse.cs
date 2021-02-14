// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Teams.Actions
{
    /// <summary>
    /// Send a tab 'auth' response.
    /// </summary>
    public class SendTabAuthResponse : BaseAuthResponseDialog
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Teams.SendTabAuthResponse";

        /// <summary>
        /// Initializes a new instance of the <see cref="SendTabAuthResponse"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public SendTabAuthResponse([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(callerPath, callerLine)
        {
            RegisterSourceLocation(callerPath, callerLine);
        }

        /// <inheritdoc/>
        protected override string OnComputeId()
        {
            return $"{GetType().Name}[{Title?.ToString() ?? string.Empty}]";
        }

        protected override Activity CreateOAuthInvokeResponseActivity(DialogContext dc, CardAction cardAction)
        {
            var responsePayload = new TabResponsePayload
            {
                Type = "auth",
                SuggestedActions = new TabSuggestedActions
                {
                    Actions = new List<CardAction>
                    {
                       cardAction,
                    },
                },
            };

            return CreateInvokeResponseActivity(new TabResponse { Tab = responsePayload });
        }
    }
}
