// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using Microsoft.Bot.Schema;
    using Microsoft.Rest;
    using Microsoft.Rest.Serialization;
    using Newtonsoft.Json;

    /// <summary>
    /// The Bot Connector REST API allows your bot to send and receive messages
    /// to channels configured in the
    /// [Bot Framework Developer Portal](https://dev.botframework.com). The
    /// Connector service uses industry-standard REST
    /// and JSON over HTTPS.
    ///
    /// Client libraries for this REST API are available. See below for a list.
    ///
    /// Many bots will use both the Bot Connector REST API and the associated
    /// [Bot State REST API](/en-us/restapi/state). The
    /// Bot State REST API allows a bot to store and retrieve state associated
    /// with users and conversations.
    ///
    /// Authentication for both the Bot Connector and Bot State REST APIs is
    /// accomplished with JWT Bearer tokens, and is
    /// described in detail in the [Connector
    /// Authentication](/en-us/restapi/authentication) document.
    ///
    /// # Client Libraries for the Bot Connector REST API
    ///
    /// * [Bot Builder for C#](/en-us/csharp/builder/sdkreference/)
    /// * [Bot Builder for Node.js](/en-us/node/builder/overview/)
    /// * Generate your own from the [Connector API Swagger
    /// file](https://raw.githubusercontent.com/Microsoft/BotBuilder/master/CSharp/Library/Microsoft.Bot.Connector.Shared/Swagger/ConnectorAPI.json)
    ///
    /// © 2016 Microsoft.
    /// </summary>
    public partial class ConnectorClient : ServiceClient<ConnectorClient>, IConnectorClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorClient"/> class.
        /// </summary>
        /// <param name='credentials'>
        /// Required. Subscription credentials which uniquely identify client subscription.
        /// </param>
        /// <param name='handlers'>
        /// Optional. The delegating handlers to add to the http client pipeline.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when a required parameter is null.
        /// </exception>
        public ConnectorClient(ServiceClientCredentials credentials, params DelegatingHandler[] handlers)
            : this(handlers)
        {
            if (credentials == null)
            {
                throw new System.ArgumentNullException(nameof(credentials));
            }

            Credentials = credentials;
            if (Credentials != null)
            {
                Credentials.InitializeServiceClient(this);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorClient"/> class.
        /// </summary>
        /// <param name='credentials'>
        /// Required. Subscription credentials which uniquely identify client subscription.
        /// </param>
        /// <param name='httpClient'>
        /// HttpClient to be used.
        /// </param>
        /// <param name='disposeHttpClient'>
        /// True: will dispose the provided httpClient on calling ConnectorClient.Dispose(). False: will not dispose provided httpClient.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when a required parameter is null.
        /// </exception>
        public ConnectorClient(ServiceClientCredentials credentials, HttpClient httpClient, bool disposeHttpClient)
            : this(httpClient, disposeHttpClient)
        {
            if (credentials == null)
            {
                throw new System.ArgumentNullException(nameof(credentials));
            }

            Credentials = credentials;
            if (Credentials != null)
            {
                Credentials.InitializeServiceClient(this);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorClient"/> class.
        /// </summary>
        /// <param name='credentials'>
        /// Required. Subscription credentials which uniquely identify client subscription.
        /// </param>
        /// <param name='rootHandler'>
        /// Optional. The http client handler used to handle http transport.
        /// </param>
        /// <param name='handlers'>
        /// Optional. The delegating handlers to add to the http client pipeline.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when a required parameter is null.
        /// </exception>
        public ConnectorClient(ServiceClientCredentials credentials, HttpClientHandler rootHandler, params DelegatingHandler[] handlers)
            : this(rootHandler, handlers)
        {
            if (credentials == null)
            {
                throw new System.ArgumentNullException(nameof(credentials));
            }

            Credentials = credentials;
            if (Credentials != null)
            {
                Credentials.InitializeServiceClient(this);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorClient"/> class.
        /// </summary>
        /// <param name='baseUri'>
        /// Optional. The base URI of the service.
        /// </param>
        /// <param name='credentials'>
        /// Required. Subscription credentials which uniquely identify client subscription.
        /// </param>
        /// <param name='handlers'>
        /// Optional. The delegating handlers to add to the http client pipeline.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when a required parameter is null.
        /// </exception>
        public ConnectorClient(System.Uri baseUri, ServiceClientCredentials credentials, params DelegatingHandler[] handlers)
            : this(handlers)
        {
            if (baseUri == null)
            {
                throw new System.ArgumentNullException(nameof(baseUri));
            }

            if (credentials == null)
            {
                throw new System.ArgumentNullException(nameof(credentials));
            }

            BaseUri = baseUri;
            Credentials = credentials;
            if (Credentials != null)
            {
                Credentials.InitializeServiceClient(this);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorClient"/> class.
        /// </summary>
        /// <param name='baseUri'>
        /// Optional. The base URI of the service.
        /// </param>
        /// <param name='credentials'>
        /// Required. Subscription credentials which uniquely identify client subscription.
        /// </param>
        /// <param name='rootHandler'>
        /// Optional. The http client handler used to handle http transport.
        /// </param>
        /// <param name='handlers'>
        /// Optional. The delegating handlers to add to the http client pipeline.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when a required parameter is null.
        /// </exception>
        public ConnectorClient(System.Uri baseUri, ServiceClientCredentials credentials, HttpClientHandler rootHandler, params DelegatingHandler[] handlers)
            : this(rootHandler, handlers)
        {
            if (baseUri == null)
            {
                throw new System.ArgumentNullException(nameof(baseUri));
            }

            if (credentials == null)
            {
                throw new System.ArgumentNullException(nameof(credentials));
            }

            BaseUri = baseUri;
            Credentials = credentials;
            if (Credentials != null)
            {
                Credentials.InitializeServiceClient(this);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorClient"/> class.
        /// </summary>
        /// <param name='httpClient'>HttpClient to be used.</param>
        /// <param name='disposeHttpClient'>
        /// True: will dispose the provided httpClient on calling ConnectorClient.Dispose(). False: will not dispose provided httpClient.</param>
        protected ConnectorClient(HttpClient httpClient, bool disposeHttpClient)
            : base(httpClient, disposeHttpClient)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorClient"/> class.
        /// </summary>
        /// <param name='handlers'>
        /// Optional. The delegating handlers to add to the http client pipeline.
        /// </param>
        protected ConnectorClient(params DelegatingHandler[] handlers)
            : base(handlers)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorClient"/> class.
        /// </summary>
        /// <param name='rootHandler'>
        /// Optional. The http client handler used to handle http transport.
        /// </param>
        /// <param name='handlers'>
        /// Optional. The delegating handlers to add to the http client pipeline.
        /// </param>
        protected ConnectorClient(HttpClientHandler rootHandler, params DelegatingHandler[] handlers)
            : base(rootHandler, handlers)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorClient"/> class.
        /// </summary>
        /// <param name='baseUri'>
        /// Optional. The base URI of the service.
        /// </param>
        /// <param name='handlers'>
        /// Optional. The delegating handlers to add to the http client pipeline.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when a required parameter is null.
        /// </exception>
        protected ConnectorClient(System.Uri baseUri, params DelegatingHandler[] handlers)
            : this(handlers)
        {
            if (baseUri == null)
            {
                throw new System.ArgumentNullException(nameof(baseUri));
            }

            BaseUri = baseUri;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorClient"/> class.
        /// </summary>
        /// <param name='baseUri'>
        /// Optional. The base URI of the service.
        /// </param>
        /// <param name='rootHandler'>
        /// Optional. The http client handler used to handle http transport.
        /// </param>
        /// <param name='handlers'>
        /// Optional. The delegating handlers to add to the http client pipeline.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when a required parameter is null.
        /// </exception>
        protected ConnectorClient(System.Uri baseUri, HttpClientHandler rootHandler, params DelegatingHandler[] handlers)
            : this(rootHandler, handlers)
        {
            if (baseUri == null)
            {
                throw new System.ArgumentNullException(nameof(baseUri));
            }

            BaseUri = baseUri;
        }
        
        /// <summary>
        /// Gets or sets the base URI of the service.
        /// </summary>
        /// <value>The has URI of the service.</value>
        public System.Uri BaseUri { get; set; }

        /// <summary>
        /// Gets the JSON serialization settings.
        /// </summary>
        /// <value>The JSON serialization settings.</value>
        public JsonSerializerSettings SerializationSettings { get; private set; }

        /// <summary>
        /// Gets the JSON deserialization settings.
        /// </summary>
        /// <value>The JSON deserialization settings.</value>
        public JsonSerializerSettings DeserializationSettings { get; private set; }

        /// <summary>
        /// Gets the subscription credentials which uniquely identify client subscription.
        /// </summary>
        /// <value>The subscription credentials.</value>
        public ServiceClientCredentials Credentials { get; private set; }

        /// <summary>
        /// Gets the IAttachments.
        /// </summary>
        /// <value>The IAttachments.</value>
        public virtual IAttachments Attachments { get; private set; }

        /// <summary>
        /// Gets the IConversations.
        /// </summary>
        /// <value>The IConversations.</value>
        public virtual IConversations Conversations { get; private set; }

        /// <summary>
        /// An optional partial-method to perform custom initialization.
        /// </summary>
        partial void CustomInitialize();

        /// <summary>
        /// Initializes client properties.
        /// </summary>
        private void Initialize()
        {
            Attachments = new Attachments(this);
            Conversations = new Conversations(this);
            BaseUri = new System.Uri("https://api.botframework.com");
            SerializationSettings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.None,
                DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc,
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Serialize,
                ContractResolver = new ReadOnlyJsonContractResolver(),
                Converters = new List<JsonConverter>
                    {
                        new Iso8601TimeSpanConverter()
                    }
            };
            DeserializationSettings = new JsonSerializerSettings
            {
                DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc,
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Serialize,
                ContractResolver = new ReadOnlyJsonContractResolver(),
                Converters = new List<JsonConverter>
                    {
                        new Iso8601TimeSpanConverter()
                    }
            };
            CustomInitialize();
        }
    }
}
