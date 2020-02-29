// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net.Http;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
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
public abstract class ConnectorClientBase : ServiceClient<ConnectorClientBase>, System.IDisposable
{
    protected ConnectorClientBase(HttpClient httpClient, bool disposeHttpClient)
        : base(httpClient, disposeHttpClient)
    {
    }

    protected ConnectorClientBase(params DelegatingHandler[] handlers) 
        : base(handlers)
    {
    }

    protected ConnectorClientBase(HttpClientHandler rootHandler, params DelegatingHandler[] handlers) 
        : base(rootHandler, handlers)
    {
    }

    /// <summary>
    /// Gets or sets the base URI of the service.
    /// </summary>
    /// <value>
    /// The base URI of the service.
    /// </value>
    public abstract System.Uri BaseUri { get; set; }

    /// <summary>
    /// Gets json serialization settings.
    /// </summary>
    /// <value>
    /// Json serialization settings.
    /// </value>
    public abstract JsonSerializerSettings SerializationSettings { get; }

    /// <summary>
    /// Gets json deserialization settings.
    /// </summary>
    /// <value>
    /// Json deserialization settings.
    /// </value>
    public abstract JsonSerializerSettings DeserializationSettings { get; }

    /// <summary>
    /// Gets subscription credentials which uniquely identify client
    /// subscription.
    /// </summary>
    /// <value>
    /// Subscription credentials which uniquely identify client
    /// subscription.
    /// </value>
    public abstract ServiceClientCredentials Credentials { get; }

    /// <summary>
    /// Gets the IAttachments.
    /// </summary>
    /// <value>
    /// The IAttachments.
    /// </value>
    public virtual IAttachments Attachments { get; }

    /// <summary>
    /// Gets the ConversationsBase.
    /// </summary>
    /// <value>
    /// The ConversationsBase.
    /// </value>
    public virtual ConversationsBase Conversations { get; }
}
