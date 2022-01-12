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
    /// Used for properly constructing a User OAuth message with OAuthCard if the channel supports it, or SignInCard.
    /// </summary>
    public class UserAuthActivityFactory
    {
        private readonly UserAuthSettings _defaultSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAuthActivityFactory"/> class.
        /// </summary>
        /// <param name="settings">Settings specific to this <see cref="UserAuthActivityFactory"/>.</param>
        public UserAuthActivityFactory(UserAuthSettings settings) 
        {
            _defaultSettings = settings ?? throw new NullReferenceException(nameof(settings));
        }

        /// <summary>
        /// This method will properly construct an OAuthCard based on <see cref="UserAuthSettings"/>
        /// provided during class construction, or from the <paramref name="promptActivity"/> if present.
        /// </summary>
        /// <param name="activity">The incoming <see cref="Activity"/> to use while constructing the card and response activity.</param>
        /// <param name="userTokenClient">The <see cref="UserTokenClient"/> to use for retrieving the <see cref="SignInResource"/> for the card.</param>
        /// <param name="settings"><see cref="UserAuthSettings"/> to use while constructing the OAuthCard.</param>
        /// <param name="promptActivity"><see cref="Activity"/> to use for adding the OAuthCard as an attachment. If this activity 
        /// does not have an attachment containing an OAuthCard, then one will be added.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use for async operations from this method.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<Activity> CreateOAuthActivityAsync(Activity activity, UserTokenClient userTokenClient, UserAuthSettings settings = default(UserAuthSettings), Activity promptActivity = default(Activity), CancellationToken cancellationToken = default(CancellationToken))
        {
            BotAssert.ActivityNotNull(activity);
            userTokenClient = userTokenClient ?? throw new ArgumentNullException(nameof(userTokenClient));
            settings = settings ?? _defaultSettings;

            // Ensure prompt initialized
            promptActivity ??= Activity.CreateMessageActivity() as Activity;

            if (promptActivity.Attachments == null)
            {
                promptActivity.Attachments = new List<Attachment>();
            }
            
            // Set input hint
            if (string.IsNullOrEmpty(promptActivity.InputHint))
            {
                promptActivity.InputHint = InputHints.AcceptingInput;
            }

            // Append appropriate card if missing
            if (!ChannelSupportsOAuthCard(activity.ChannelId))
            {
                await AddSignInCardToActivityAsync(promptActivity, activity, userTokenClient, settings, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await AddOAuthCardToActivityAsync(promptActivity, activity, userTokenClient, settings, cancellationToken).ConfigureAwait(false);
            }

            return promptActivity;
        }

        private static async Task AddOAuthCardToActivityAsync(Activity promptActivity, Activity originalActivity, UserTokenClient userTokenClient, UserAuthSettings settings, CancellationToken cancellationToken)
        {
            if (!promptActivity.Attachments.Any(a => a.Content is OAuthCard))
            {
                var cardActionType = ActionTypes.Signin;
                var signInResource = await userTokenClient.GetSignInResourceAsync(settings.ConnectionName, originalActivity, null, cancellationToken).ConfigureAwait(false);
                var value = signInResource.SignInLink;

                // use the SignInLink when 
                //   in speech channel or
                //   bot is a skill or
                //   an extra OAuthAppCredentials is being passed in
                if (originalActivity.IsFromStreamingConnection() ||

                    // TODO: support skills with emulator
                    //(turnContext.TurnState.Get<ClaimsIdentity>(BotAdapter.BotIdentityKey) is ClaimsIdentity botIdentity && botIdentity.Claims.IsSkillClaim()) ||
                    settings.OAuthAppCredentials != null)
                {
                    if (originalActivity.ChannelId == Channels.Emulator)
                    {
                        cardActionType = ActionTypes.OpenUrl;
                    }
                }
                else if ((settings.ShowSignInLink != null && settings.ShowSignInLink == false) ||
                    (settings.ShowSignInLink == null && !ChannelRequiresSignInLink(originalActivity.ChannelId)))
                {
                    value = null;
                }

                promptActivity.Attachments.Add(new Attachment
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
        }

        private static async Task AddSignInCardToActivityAsync(Activity promptActivity, Activity originalActivity, UserTokenClient userTokenClient, UserAuthSettings settings, CancellationToken cancellationToken)
        {
            if (!promptActivity.Attachments.Any(a => a.Content is SigninCard))
            {
                var signInResource = await userTokenClient.GetSignInResourceAsync(settings.ConnectionName, originalActivity, null, cancellationToken).ConfigureAwait(false);
                promptActivity.Attachments.Add(new Attachment
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
