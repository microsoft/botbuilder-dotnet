// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Names for signin invoke operations in the token protocol.
    /// </summary>
    public static class SignInConstants
    {
        /// <summary>
        /// Name for the signin invoke to verify the 6-digit authentication code as part of sign-in.
        /// </summary>
        /// <remarks>
        /// This invoke operation includes a value containing a state property for the magic code.
        /// </remarks>
        public const string VerifyStateOperationName = "signin/verifyState";

        /// <summary>
        /// Name for signin invoke to perform a token exchange.
        /// </summary>
        /// <remarks>
        /// This invoke operation includes a value of the token exchange class.
        /// </remarks>
        public const string TokenExchangeOperationName = "signin/tokenExchange";

        /// <summary>
        /// The EventActivity name when a token is sent to the bot.
        /// </summary>
        public const string TokenResponseEventName = "tokens/response";
    }
}
