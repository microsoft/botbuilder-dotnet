// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Newtonsoft.Json;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// MicrosoftAppCredentials auth implementation and cache.
    /// </summary>
    public class MicrosoftAppCredentials : AppCredentials
    {
        /// <summary>
        /// The configuration property for the Microsoft app Password.
        /// </summary>
        public const string MicrosoftAppPasswordKey = "MicrosoftAppPassword";

        /// <summary>
        /// The configuration property for the Microsoft app ID.
        /// </summary>
        public const string MicrosoftAppIdKey = "MicrosoftAppId";

        /// <summary>
        /// An empty set of credentials.
        /// </summary>
        public static readonly MicrosoftAppCredentials Empty = new MicrosoftAppCredentials(null, null);

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftAppCredentials"/> class.
        /// </summary>
        /// <param name="appId">The Microsoft app ID.</param>
        /// <param name="password">The Microsoft app password.</param>
        public MicrosoftAppCredentials(string appId, string password)
            : this(appId, password, null, null, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftAppCredentials"/> class.
        /// </summary>
        /// <param name="appId">The Microsoft app ID.</param>
        /// <param name="password">The Microsoft app password.</param>
        /// <param name="customHttpClient">Optional <see cref="HttpClient"/> to be used when acquiring tokens.</param>
        public MicrosoftAppCredentials(string appId, string password, HttpClient customHttpClient)
            : this(appId, password, null, customHttpClient)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftAppCredentials"/> class.
        /// </summary>
        /// <param name="appId">The Microsoft app ID.</param>
        /// <param name="password">The Microsoft app password.</param>
        /// <param name="customHttpClient">Optional <see cref="HttpClient"/> to be used when acquiring tokens.</param>
        /// <param name="logger">Optional <see cref="ILogger"/> to gather telemetry data while acquiring and managing credentials.</param>
        public MicrosoftAppCredentials(string appId, string password, HttpClient customHttpClient, ILogger logger)
            : this(appId, password, null, customHttpClient, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftAppCredentials"/> class.
        /// </summary>
        /// <param name="appId">The Microsoft app ID.</param>
        /// <param name="password">The Microsoft app password.</param>
        /// <param name="customHttpClient">Optional <see cref="HttpClient"/> to be used when acquiring tokens.</param>
        /// <param name="logger">Optional <see cref="ILogger"/> to gather telemetry data while acquiring and managing credentials.</param>
        /// <param name="oAuthScope">The scope for the token.</param>
        public MicrosoftAppCredentials(string appId, string password, HttpClient customHttpClient, ILogger logger,  string oAuthScope)
            : this(appId, password, null, customHttpClient, logger, oAuthScope)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftAppCredentials"/> class.
        /// </summary>
        /// <param name="appId">The Microsoft app ID.</param>
        /// <param name="password">The Microsoft app password.</param>
        /// <param name="channelAuthTenant">Optional. The oauth token tenant.</param>
        /// <param name="customHttpClient">Optional <see cref="HttpClient"/> to be used when acquiring tokens.</param>
        public MicrosoftAppCredentials(string appId, string password, string channelAuthTenant, HttpClient customHttpClient)
            : this(appId, password, channelAuthTenant, customHttpClient, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftAppCredentials"/> class.
        /// </summary>
        /// <param name="appId">The Microsoft app ID.</param>
        /// <param name="password">The Microsoft app password.</param>
        /// <param name="channelAuthTenant">Optional. The oauth token tenant.</param>
        /// <param name="customHttpClient">Optional <see cref="HttpClient"/> to be used when acquiring tokens.</param>
        /// <param name="logger">Optional <see cref="ILogger"/> to gather telemetry data while acquiring and managing credentials.</param>
        public MicrosoftAppCredentials(string appId, string password, string channelAuthTenant, HttpClient customHttpClient, ILogger logger = null)
            : this(appId, password, channelAuthTenant, customHttpClient, logger, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftAppCredentials"/> class.
        /// </summary>
        /// <param name="appId">The Microsoft app ID.</param>
        /// <param name="password">The Microsoft app password.</param>
        /// <param name="channelAuthTenant">Optional. The oauth token tenant.</param>
        /// <param name="customHttpClient">Optional <see cref="HttpClient"/> to be used when acquiring tokens.</param>
        /// <param name="logger">Optional <see cref="ILogger"/> to gather telemetry data while acquiring and managing credentials.</param>
        /// <param name="oAuthScope">The scope for the token.</param>
        public MicrosoftAppCredentials(string appId, string password, string channelAuthTenant, HttpClient customHttpClient, ILogger logger = null, string oAuthScope = null)
            : base(channelAuthTenant, customHttpClient, logger, oAuthScope)
        {
            MicrosoftAppId = appId;
            MicrosoftAppPassword = password;
        }

        /// <summary>
        /// Gets or sets the Microsoft app password for this credential.
        /// </summary>
        /// <value>
        /// The Microsoft app password for this credential.
        /// </value>
        public string MicrosoftAppPassword { get; set; }

        /// <summary>
        /// Builds the lazy <see cref="AdalAuthenticator" /> to be used for token acquisition.
        /// </summary>
        /// <returns>A lazy <see cref="AdalAuthenticator"/>.</returns>
        protected override Lazy<AdalAuthenticator> BuildAuthenticator()
        {
            return new Lazy<AdalAuthenticator>(
                () =>
                new AdalAuthenticator(
                    new ClientCredential(MicrosoftAppId, MicrosoftAppPassword),
                    new OAuthConfiguration() { Authority = OAuthEndpoint, Scope = OAuthScope },
                    this.CustomHttpClient,
                    this.Logger),
                LazyThreadSafetyMode.ExecutionAndPublication);
        }
    }
}
