// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector
{
    using Microsoft.Rest;
    using Microsoft.Bot.Schema;
    using Newtonsoft.Json;

    /// <summary>
    /// </summary>
    public partial interface IOAuthClient : System.IDisposable
    {
        /// <summary>
        /// The base URI of the service.
        /// </summary>
        System.Uri BaseUri { get; set; }

        /// <summary>
        /// Gets or sets json serialization settings.
        /// </summary>
        JsonSerializerSettings SerializationSettings { get; }

        /// <summary>
        /// Gets or sets json deserialization settings.
        /// </summary>
        JsonSerializerSettings DeserializationSettings { get; }

        /// <summary>
        /// Subscription credentials which uniquely identify client
        /// subscription.
        /// </summary>
        ServiceClientCredentials Credentials { get; }


        /// <summary>
        /// Gets the IBotSignIn.
        /// </summary>
        IBotSignIn BotSignIn { get; }

        /// <summary>
        /// Gets the IUserToken.
        /// </summary>
        IUserToken UserToken { get; }

    }
}
