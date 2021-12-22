// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.OAuth
{
    /// <summary>
    /// Used for properly constructing a User OAuth message with OAuthCard of the channel supports it, or SignInCard.
    /// </summary>
    public class OAuthMessageClient
    {
        private readonly OAuthSettings _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthMessageClient"/> class.
        /// </summary>
        public OAuthMessageClient() 
        { 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthMessageClient"/> class.
        /// </summary>
        /// <param name="settings">Settings specific to this <see cref="OAuthMessageClient"/>.</param>
        public OAuthMessageClient(OAuthSettings settings) 
        {
            _settings = settings ?? throw new NullReferenceException(nameof(settings));
        }

        /// <summary>
        /// Useful for determining if an activity is an Azure Bot Service response to an OAuthCard.
        /// </summary>
        /// <param name="activity">The activity to check the type and name of.</param>
        /// <returns>True if the activity is of type event with name of tokens/response or an invoke
        /// with name of signin/verifyState or signin/tokenExchange.</returns>
        public static bool IsOAuthResponseActivity(Activity activity)
        {
            return (activity.Type.Equals(ActivityTypes.Event, StringComparison.OrdinalIgnoreCase) && activity.Name.Equals(SignInConstants.TokenResponseEventName, StringComparison.OrdinalIgnoreCase))
                || (activity.Type.Equals(ActivityTypes.Invoke, StringComparison.OrdinalIgnoreCase) && activity.Name.Equals(SignInConstants.VerifyStateOperationName, StringComparison.OrdinalIgnoreCase))
                || (activity.Type.Equals(ActivityTypes.Invoke, StringComparison.OrdinalIgnoreCase) && activity.Name.Equals(SignInConstants.TokenExchangeOperationName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// This method will properly construct an OAuthCard based on <see cref="OAuthSettings"/>
        /// provided during class construction, or from the <paramref name="prompt"/> if present.
        /// </summary>
        /// <param name="activity">The incoming <see cref="Activity"/> to use while constructing the card and response activity.</param>
        /// <param name="userTokenClient">The <see cref="UserTokenClient"/> to use for retrieving the <see cref="SignInResource"/> for the card.</param>
        /// <param name="settings"><see cref="OAuthSettings"/> to use while constructing the OAuthCard.</param>
        /// <param name="prompt"><see cref="IMessageActivity"/> to use for sending the OAuthCard. If this activity 
        /// does not have an attachment containing an OAuthCard, then one will be added.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use for async operations from this method.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IMessageActivity> GetCardMessageFromActivityAsync(Activity activity, UserTokenClient userTokenClient, OAuthSettings settings = default(OAuthSettings), IMessageActivity prompt = default(IMessageActivity), CancellationToken cancellationToken = default(CancellationToken))
        {
            BotAssert.ActivityNotNull(activity);
            userTokenClient = userTokenClient ?? throw new ArgumentNullException(nameof(userTokenClient));
            settings = settings ?? _settings ?? throw new NullReferenceException(nameof(settings));

            // Ensure prompt initialized
            prompt ??= Activity.CreateMessageActivity();

            if (prompt.Attachments == null)
            {
                prompt.Attachments = new List<Attachment>();
            }

            // Append appropriate card if missing
            if (!ChannelSupportsOAuthCard(activity.ChannelId))
            {
                if (!prompt.Attachments.Any(a => a.Content is SigninCard))
                {
                    var signInResource = await userTokenClient.GetSignInResourceAsync(settings.ConnectionName, activity, null, cancellationToken).ConfigureAwait(false);
                    prompt.Attachments.Add(new Attachment
                    {
                        ContentType = SigninCard.ContentType,
                        Content = new SigninCard
                        {
                            Text = settings.Text,
                            Buttons = new[]
                            {
                                new CardAction
                                {
                                    Title = settings.Title,
                                    Value = signInResource.SignInLink,
                                    Type = ActionTypes.Signin,
                                },
                            },
                        },
                    });
                }
            }
            else if (!prompt.Attachments.Any(a => a.Content is OAuthCard))
            {
                var cardActionType = ActionTypes.Signin;
                var signInResource = await userTokenClient.GetSignInResourceAsync(settings.ConnectionName, activity, null, cancellationToken).ConfigureAwait(false);
                var value = signInResource.SignInLink;

                // use the SignInLink when 
                //   in speech channel or
                //   bot is a skill or
                //   an extra OAuthAppCredentials is being passed in
                if (activity.IsFromStreamingConnection() ||

                    // TODO: support skills with emulator
                    //(turnContext.TurnState.Get<ClaimsIdentity>(BotAdapter.BotIdentityKey) is ClaimsIdentity botIdentity && botIdentity.Claims.IsSkillClaim()) ||
                    settings.OAuthAppCredentials != null)
                {
                    if (activity.ChannelId == Channels.Emulator)
                    {
                        cardActionType = ActionTypes.OpenUrl;
                    }
                }
                else if ((settings.ShowSignInLink != null && settings.ShowSignInLink == false) ||
                    (settings.ShowSignInLink == null && !ChannelRequiresSignInLink(activity.ChannelId)))
                {
                    value = null;
                }

                prompt.Attachments.Add(new Attachment
                {
                    ContentType = OAuthCard.ContentType,
                    Content = new OAuthCard
                    {
                        Text = settings.Text,
                        ConnectionName = settings.ConnectionName,
                        Buttons = new[]
                        {
                            new CardAction
                            {
                                Title = settings.Title,
                                Text = settings.Text,
                                Type = cardActionType,
                                Value = value
                            },
                        },
                        TokenExchangeResource = signInResource.TokenExchangeResource,
                    },
                });
            }

            // Set input hint
            if (string.IsNullOrEmpty(prompt.InputHint))
            {
                prompt.InputHint = InputHints.AcceptingInput;
            }
            
            return prompt;
        }

        private static bool ChannelSupportsOAuthCard(string channelId)
        {
            switch (channelId)
            {
                case Channels.Cortana:
                case Channels.Skype:
                case Channels.Skypeforbusiness:
                    return false;
            }

            return true;
        }

        private static bool ChannelRequiresSignInLink(string channelId)
        {
            switch (channelId)
            {
                case Channels.Msteams:
                    return true;
            }

            return false;
        }
    }
}
