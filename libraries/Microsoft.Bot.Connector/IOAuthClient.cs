// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector
{
    using Microsoft.Bot.Schema;
    using Microsoft.Rest;
    using Newtonsoft.Json;

    /// <summary>IOAuthClient. </summary>
    public partial interface IOAuthClient : System.IDisposable
    {
        /// <summary> Gets or sets the base URI of the service. </summary>
        /// <value>The base URI.</value>
        System.Uri BaseUri { get; set; }

        /// <summary> Gets json serialization settings. </summary>
        /// <value>The serialization settings.</value>
        JsonSerializerSettings SerializationSettings { get; }

        /// <summary> Gets the json deserialization settings. </summary>
        /// <value>The deserialization settings.</value>
        JsonSerializerSettings DeserializationSettings { get; }

        /// <summary> Gets subscription credentials which uniquely identify client subscription. </summary>
        /// <value> The client credentials.</value>
        ServiceClientCredentials Credentials { get; }

        /// <summary> Gets the IBotSignIn. </summary>
        /// <value> The bot sign-in. </value>
        IBotSignIn BotSignIn { get; }

        /// <summary> Gets the IUserToken. </summary>
        /// <value>The <see cref="UserToken"/>.</value>
        IUserToken UserToken { get; }
    }
}
