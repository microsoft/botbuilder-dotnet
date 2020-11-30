// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema;
    using Microsoft.Rest;

    /// <summary>
    /// BotSignIn operations.
    /// </summary>
    public partial interface IBotSignIn
    {
        /// <summary>Get sign-in URL with HTTP Message.</summary>
        /// <param name='state'>State.</param>
        /// <param name='codeChallenge'>Code challenge.</param>
        /// <param name='emulatorUrl'>Emulator URL.</param>
        /// <param name='finalRedirect'>Final redirect.</param>
        /// <param name='customHeaders'>The headers that will be added to request.</param>
        /// <param name='cancellationToken'>The cancellation token.</param>
        /// <exception cref="Microsoft.Rest.HttpOperationException">Thrown when the operation returned an invalid status code.</exception>
        /// <exception cref="Microsoft.Rest.SerializationException">Thrown when unable to deserialize the response.</exception>
        /// <exception cref="Microsoft.Rest.ValidationException">Thrown when a required parameter is null.</exception>
        /// <returns>A task that represents the work queued to execute.</returns>
        Task<HttpOperationResponse<string>> GetSignInUrlWithHttpMessagesAsync(string state, string codeChallenge = default(string), string emulatorUrl = default(string), string finalRedirect = default(string), Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}
