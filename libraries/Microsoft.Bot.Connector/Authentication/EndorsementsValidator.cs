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
        /// Verify that a channel matches the endorsements found on the JWT token.
        /// For example, if an <see cref="Schema.Activity"/> comes from WebChat, that activity's
        /// <see cref="Schema.Activity.ChannelId"/> property is set to "webchat" and the signing party
        /// of the JWT token must have a corresponding endorsement of “Webchat”.
        /// </summary>
        /// <param name="channelId">The ID of the channel to validate, typically extracted from the activity's
        /// <see cref="Schema.Activity.ChannelId"/> property, that to which the Activity is affinitized.</param>
        /// <param name="endorsements">The JWT token’s signing party is permitted to send activities only for
        /// specific channels. That list, the set of channels the service can sign for, is called the the endorsement list.
        /// The activity’s <see cref="Schema.Activity.ChannelId"/> MUST be found in the endorsement list, or the incoming 
        /// activity is not considered valid.</param>
        /// <returns>True if the channel ID is found in the endorsements list; otherwise, false.</returns>
        public static bool Validate(string channelId, HashSet<string> endorsements)
        {
            // If the Activity came in and doesn't have a channel ID then it's making no 
            // assertions as to who endorses it. This means it should pass. 
            if (string.IsNullOrEmpty(channelId))
                return true;

            if (endorsements == null)
                throw new ArgumentNullException(nameof(endorsements));

            // The Call path to get here is: 
            // JwtTokenValidation.AuthenticateRequest
            //  ->
            //   JwtTokenValidation.ValidateAuthHeader
            //    ->                                         
            //      ChannelValidation.AuthenticateChannelToken
            //       -> 
            //          JWTTokenExtractor

            // Does the set of endorsements match the channelId that was passed in?
            var endorsementPresent = endorsements.Contains(channelId);
            return endorsementPresent;
        }
    }
}
