﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector.Teams
{
    using Microsoft.Rest;
    using Microsoft.Bot.Schema.Teams;
    using Newtonsoft.Json;

    /// <summary>
    /// ﻿﻿The Bot Connector REST API extension for Microsoft Teams allows your
    /// bot to perform extended operations on to Microsoft Teams channel
    /// configured in the
    /// [Bot Framework Developer Portal](https://dev.botframework.com). The
    /// Connector service uses industry-standard REST and JSON over HTTPS.
    ///
    /// Client libraries for this REST API are available. See below for a list.
    ///
    ///
    ///
    /// Authentication for both the Bot Connector and Bot State REST APIs is
    /// accomplished with JWT Bearer tokens, and is
    /// described in detail in the [Connector
    /// Authentication](https://docs.botframework.com/en-us/restapi/authentication)
    /// document.
    ///
    /// # Client Libraries for the Bot Connector REST API
    ///
    /// * [Bot Builder for
    /// C#](https://docs.botframework.com/en-us/csharp/builder/sdkreference/)
    /// * [Bot Builder for
    /// Node.js](https://docs.botframework.com/en-us/node/builder/overview/)
    ///
    /// © 2016 Microsoft
    /// </summary>
    public partial interface ITeamsConnectorClient : System.IDisposable
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
        /// Gets the ITeamsOperations.
        /// </summary>
        ITeamsOperations Teams { get; }

    }
}
