﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector
{
    using Microsoft.Rest;
    using Microsoft.Bot.Schema;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// BotSignIn operations.
    /// </summary>
    public partial interface IBotSignIn
    {
        /// <param name='state'>
        /// </param>
        /// <param name='codeChallenge'>
        /// </param>
        /// <param name='emulatorUrl'>
        /// </param>
        /// <param name='finalRedirect'>
        /// </param>
        /// <param name='customHeaders'>
        /// The headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
        /// </exception>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null
        /// </exception>
        Task<HttpOperationResponse<string>> GetSignInUrlWithHttpMessagesAsync(string state, string codeChallenge = default(string), string emulatorUrl = default(string), string finalRedirect = default(string), Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}
