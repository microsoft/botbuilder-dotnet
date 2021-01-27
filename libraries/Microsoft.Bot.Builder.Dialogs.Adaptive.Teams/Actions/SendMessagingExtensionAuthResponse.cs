// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Teams.Actions;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Teams.Actions
{
    /// <summary>
    /// Send a messaging extension 'auth' response.
    /// </summary>
    public class SendMessagingExtensionAuthResponse : BaseTeamsCacheInfoResponseDialog
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Teams.SendMessagingExtensionAuthResponse";

        /// <summary>
        /// Initializes a new instance of the <see cref="SendMessagingExtensionAuthResponse"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public SendMessagingExtensionAuthResponse([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets property path to put the TokenResponse value in once retrieved.
        /// </summary>
        /// <value>
        /// Property path to put the value in.
        /// </value>
        [JsonProperty("resultProperty")]
        public StringExpression Property { get; set; }

        /// <summary>
        /// Gets or sets the name of the OAuth connection.
        /// </summary>
        /// <value>String or expression which evaluates to a connection string.</value>
        [JsonProperty("connectionName")]
        public StringExpression ConnectionName { get; set; }

        /// <summary>
        /// Gets or sets an Title of the response.
        /// </summary>
        /// <value>
        /// An string or expression which evaluates to a string for the response title.
        /// </value>
        [JsonProperty("title")]
        public StringExpression Title { get; set; }

        /// <summary>
        /// Called when the dialog is started and pushed onto the dialog stack.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="options">Optional, initial information to pass to the dialog.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            if (!(dc.Context.Adapter is IExtendedUserTokenProvider tokenProvider))
            {
                throw new InvalidOperationException($"{Kind}: not supported by the current adapter");
            }

            if (Disabled != null && Disabled.GetValue(dc.State) == true)
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            var connectionName = ConnectionName.GetValueOrNull(dc.State);
            if (string.IsNullOrEmpty(connectionName))
            {
                throw new InvalidOperationException($"A valid {nameof(ConnectionName)} is required for {Kind}.");
            }

            var title = Title.GetValueOrNull(dc.State);
            if (string.IsNullOrEmpty(title))
            {
                throw new InvalidOperationException($"A valid {nameof(Title)} is required for {Kind}.");
            }

            var tokenResponse = await GetUserTokenAsync(dc, tokenProvider, connectionName, cancellationToken).ConfigureAwait(false);
            if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.Token))
            {
                // we have the token, so the user is already signed in.
                // Similar to OAuthInput, just return the token in the property.
                if (Property != null)
                {
                    dc.State.SetValue(Property.GetValue(dc.State), tokenResponse);
                }

                // End the dialog and return the token response
                return await dc.EndDialogAsync(tokenResponse, cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            // There is no token, so the user has not signed in yet.
            var activity = await GetInvokeResponseWithSignInLinkAsync(dc, title, tokenProvider, connectionName, cancellationToken).ConfigureAwait(false);
            await dc.Context.SendActivityAsync(activity, cancellationToken: cancellationToken).ConfigureAwait(false);

            // Since a token was not retrieved above, end the turn.
            return Dialog.EndOfTurn;
        }

        /// <summary>
        /// Builds the compute Id for the dialog.
        /// </summary>
        /// <returns>A string representing the compute Id.</returns>
        protected override string OnComputeId()
        {
            return $"{GetType().Name}[{Title?.ToString() ?? string.Empty}]";
        }

        private static Task<TokenResponse> GetUserTokenAsync(DialogContext dc, IExtendedUserTokenProvider tokenProvider, string connectionName, CancellationToken cancellationToken)
        {
            // When the Bot Service Auth flow completes, the action.State will contain a magic code used for verification.
            string state = null;
            if (dc.Context.Activity.Value is JObject valueAsObject)
            {
                state = valueAsObject.Value<string>("state");
            }

            string magicCode = null;
            if (!string.IsNullOrEmpty(state))
            {
                if (int.TryParse(state, out var parsed))
                {
                    magicCode = parsed.ToString(CultureInfo.InvariantCulture);
                }
            }

            // TODO: SSO and skills token exchange

            return tokenProvider.GetUserTokenAsync(dc.Context, connectionName, magicCode, cancellationToken: cancellationToken);
        }

        private async Task<Activity> GetInvokeResponseWithSignInLinkAsync(DialogContext dc, string title, IExtendedUserTokenProvider tokenProvider, string connectionName, CancellationToken cancellationToken)
        {
            // Retrieve the OAuth Sign in Link to use in the MessagingExtensionResult Suggested Actions
            var signInLink = await tokenProvider.GetOauthSignInLinkAsync(dc.Context, connectionName, cancellationToken: cancellationToken).ConfigureAwait(false);

            var result = new MessagingExtensionResult
            {
                Type = MessagingExtensionResultResponseType.auth.ToString(),
                SuggestedActions = new MessagingExtensionSuggestedAction
                {
                    Actions = new List<CardAction>
                                {
                                    new CardAction
                                    {
                                        Type = ActionTypes.OpenUrl,
                                        Value = signInLink,
                                        Title = title,
                                    },
                                },
                },
            };

            return CreateMessagingExtensionInvokeResponseActivity(dc, result);
        }
    }
}
