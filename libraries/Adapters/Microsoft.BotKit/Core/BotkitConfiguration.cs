// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder;

namespace Microsoft.BotKit.Core
{
    /// <summary>
    /// The AdapterKey properties.
    /// </summary>
    public enum AdapterKey
    {
        /// <summary>
        /// The AppId.
        /// </summary>
        AppID,

        /// <summary>
        /// The AppPassword.
        /// </summary>
        AppPassword,

        /// <summary>
        /// The ChannelAuthTenant.
        /// </summary>
        ChannelAuthTenant,

        /// <summary>
        /// The ChannelService.
        /// </summary>
        ChannelService,

        /// <summary>
        /// The Authorization endpoint.
        /// </summary>
        OAuthEndpoint,

        /// <summary>
        /// The openIdMetadada.
        /// </summary>
        OpenIdMetadata,
    }

    /// <summary>
    /// Defines the options used when instantiating Botkit to create the main app controller with `new Botkit(options)`.
    /// </summary>
    public class BotkitConfiguration
    {
        /// <summary>
        /// Gets or sets the path used to create incoming webhook URI. Defaults to `/api/messages`.
        /// </summary>
        public string WebhookUri { get; set; }

        /// <summary>
        /// Gets or sets the name of the dialogState property in the ConversationState that will be used to automatically track the dialog state. Defaults to `dialogState`.
        /// </summary>
        public string DialogStateProperty { get; set; }

        /// <summary>
        /// Gets or sets a fully configured BotBuilder Adapter, such as `botbuilder-adapter-slack` or `botbuilder-adapter-web`.
        /// The adapter is responsible for translating platform-specific messages into the format understood by Botkit and BotBuilder.
        /// </summary>
        public BotkitBotFrameworkAdapter Adapter { get; set; } // TO-DO: compare with TS implementation

        /// <summary>
        /// Gets or sets the options that will be passed to the new Adapter when created internally.
        /// </summary>
        public Tuple<AdapterKey, string> AdapterConfig { get; set; }

        /// <summary>
        /// Gets or sets an instance of Express used to define web endpoints. If not specified, it will be created internally.
        /// Note: only use your own Express if you absolutely must for some reason. Otherwise, use `controller.webserver`.
        /// </summary>
        public IWebserver Webserver { get; set; }

        /// <summary>
        /// Gets or sets a storage interface. Defaults to the ephemeral `MemoryStorage`.
        /// </summary>
        public IStorage Storage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether if Botkit will not create a webserver or expose any webhook endpoints automatically or it Will. Defaults to false.
        /// </summary>
        public bool DisableWebserver { get; set; }
    }
}
