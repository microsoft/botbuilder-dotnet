// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector.Client.Authentication
{
    /// <summary>
    /// An interface used to validate identity <see cref="Claim"/>.
    /// </summary>
    public abstract class ClaimsValidator
    {
        /// <summary>
        /// Validates a list of <see cref="Claim"/> and should throw an exception if the validation fails.
        /// </summary>
        /// <param name="claims">The list of claims to validate.</param>
        /// <returns>true if the validation is successful, false if not.</returns>
        /// <exception cref="UnauthorizedAccessException">Throw this exception if the validation fails.</exception>
        public abstract Task ValidateClaimsAsync(IList<Claim> claims);
    }
}
