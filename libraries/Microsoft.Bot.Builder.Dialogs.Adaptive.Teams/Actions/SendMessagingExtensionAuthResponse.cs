// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Send a messaging extension 'auth' message in response..
    /// </summary>
    public class SendMessagingExtensionAuthResponse : Dialog
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Teams.SendMessagingExtensionAuthResponse";

        /// <summary>
        /// Initializes a new instance of the <see cref="SendMessagingExtensionAuthResponse"/> class.
        /// </summary>
        /// <param name="title">Text template to use for creating auth window title.</param>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public SendMessagingExtensionAuthResponse(string title = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            if (title != null)
            {
                Title = new TextTemplate(title);
            }
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
        /// Gets or sets property path to put the value in.
        /// </summary>
        /// <value>
        /// Property path to put the value in.
        /// </value>
        [JsonProperty("property")]
        public StringExpression Property { get; set; }

        /// <summary>
        /// Gets or sets the name of the OAuth connection.
        /// </summary>
        /// <value>String or expression which evaluates to a string.</value>
        [JsonProperty("connectionName")]
        public StringExpression ConnectionName { get; set; }

        /// <summary>
        /// Gets or sets an optional expression for the Title the Task Module response.
        /// </summary>
        /// <value>
        /// An string expression. 
        /// </value>
        [JsonProperty("title")]
        public ITemplate<string> Title { get; set; }

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

            var connectionName = ConnectionName.GetValue(dc.State);
            if (string.IsNullOrEmpty(connectionName))
            {
                throw new InvalidOperationException("Messaging Extension Auth Response requires a Connection Name.");
            }

            var title = await Title.BindAsync(dc, dc.State, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrEmpty(title))
            {
                throw new InvalidOperationException("Messaging Extension Auth Response requires a Title.");
            }

            // When the Bot Service Auth flow completes, the action.State will contain a magic code used for verification.
            string state = null;
            var valueAsOjbect = dc.Context.Activity.Value as JObject;
            if (valueAsOjbect != null)
            {
                state = valueAsOjbect.Value<string>("state");
            }

            string magicCode = null;
            if (!string.IsNullOrEmpty(state))
            {
                int parsed = 0;
                if (int.TryParse(state, out parsed))
                {
                    magicCode = parsed.ToString(CultureInfo.InvariantCulture);
                }
            }
            
            // TODO: SSO and skills token exchange

            var response = await (dc.Context.Adapter as IUserTokenProvider).GetUserTokenAsync(dc.Context, connectionName, magicCode, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (response != null && !string.IsNullOrEmpty(response.Token))
            {
                // we have the token, so the user is already signed in.
                // Similar to OAuthInput, just return the token in the property.    
                dc.State.SetValue(this.Property.GetValue(dc.State), response);
                if (this.Property != null)
                {
                    dc.State.SetValue(this.Property.GetValue(dc.State), response);
                }

                // End the dialog and return the token response
                return await dc.EndDialogAsync(response, cancellationToken).ConfigureAwait(false);
            }

            // There is no token, so the user has not signed in yet.

            // Retrieve the OAuth Sign in Link to use in the MessagingExtensionResult Suggested Actions
            var signInLink = await (dc.Context.Adapter as IUserTokenProvider).GetOauthSignInLinkAsync(dc.Context, connectionName, cancellationToken).ConfigureAwait(false);

            var activity = new Activity
            {
                Value = new InvokeResponse
                {
                    Status = (int)HttpStatusCode.OK,
                    Body = new MessagingExtensionResponse
                    {
                        ComposeExtension = new MessagingExtensionResult
                        {
                            Type = "auth",
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
                        },
                    }
                },
                Type = ActivityTypesEx.InvokeResponse
            };

            var properties = new Dictionary<string, string>()
            {
                { "SendMessagingExtensionAuthResponse", activity.ToString() },
                { "title", title ?? string.Empty },
            };
            TelemetryClient.TrackEvent("GeneratorResult", properties);

            await dc.Context.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);

            // Since a token was not retrieved above, end the turn.
            return Dialog.EndOfTurn;
        }

        /// <summary>
        /// Builds the compute Id for the dialog.
        /// </summary>
        /// <returns>A string representing the compute Id.</returns>
        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}[{this.Title?.ToString() ?? string.Empty}]";
        }
    }
}
