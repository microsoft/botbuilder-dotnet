// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Contains helper methods for verifying JWT endorsements.
    /// </summary>
    public static class EndorsementsValidator
    {
        /// <summary>
        /// Verify that the specified endorsement exists on the JWT token. Call this method multiple times to validate multiple endorsements.
        /// For example, if an <see cref="Schema.Activity"/> comes from WebChat, that activity's
        /// <see cref="Schema.Activity.ChannelId"/> property is set to "webchat" and the signing party
        /// of the JWT token must have a corresponding endorsement of “Webchat”.
        /// </summary>
        /// <remarks>
        /// JWT token signing keys contain endorsements matching the IDs of the channels they are approved to sign for.
        /// They also contain keywords representing compliance certifications. This code ensures that a channel ID or compliance
        /// certification is present on the signing key used for the request's token.
        /// </remarks>
        /// <param name="expectedEndorsement">The expected endorsement. Generally the ID of the channel to validate, typically extracted from the activity's
        /// <see cref="Schema.Activity.ChannelId"/> property, that to which the Activity is affinitized. Alternatively, it could represent a compliance certification that is required.</param>
        /// <param name="endorsements">The JWT token’s signing party is permitted to send activities only for
        /// specific channels. That list, the set of channels the service can sign for, is called the endorsement list.
        /// The activity’s <see cref="Schema.Activity.ChannelId"/> MUST be found in the endorsement list, or the incoming
        /// activity is not considered valid.</param>
        /// <returns>True if the channel ID is found in the endorsements list; otherwise, false.</returns>
        public static bool Validate(string expectedEndorsement, HashSet<string> endorsements)
        {
            // If the Activity came in and doesn't have a channel ID then it's making no
            // assertions as to who endorses it. This means it should pass.
            if (string.IsNullOrEmpty(expectedEndorsement))
            {
                return true;
            }

            if (endorsements == null)
            {
                throw new ArgumentNullException(nameof(endorsements));
            }

            // The Call path to get here is:
            // JwtTokenValidation.AuthenticateRequest
            //  ->
            //   JwtTokenValidation.ValidateAuthHeader
            //    ->
            //      ChannelValidation.AuthenticateChannelToken
            //       ->
            //          JWTTokenExtractor

            // Does the set of endorsements match the channelId that was passed in?
            var endorsementPresent = endorsements.Contains(expectedEndorsement);
            return endorsementPresent;
        }
    }
}
