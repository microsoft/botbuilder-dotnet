// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Messaging extension query.
    /// </summary>
    public class MessagingExtensionQuery
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingExtensionQuery"/> class.
        /// </summary>
        /// <param name="commandId">Id of the command assigned by Bot.</param>
        /// <param name="parameters">Parameters for the query.</param>
        /// <param name="queryOptions">The query options.</param>
        /// <param name="state">State parameter passed back to the bot after authentication/configuration flow.</param>
        public MessagingExtensionQuery(string commandId = default, IList<MessagingExtensionParameter> parameters = default, MessagingExtensionQueryOptions queryOptions = default, string state = default)
        {
            CommandId = commandId;
            Parameters = parameters ?? new List<MessagingExtensionParameter>();
            QueryOptions = queryOptions;
            State = state;
        }

        /// <summary>
        /// Gets or sets ID of the command assigned by Bot.
        /// </summary>
        /// <value>The ID of the command assigned by the Bot.</value>
        [JsonProperty(PropertyName = "commandId")]
        public string CommandId { get; set; }

        /// <summary>
        /// Gets parameters for the query.
        /// </summary>
        /// <value>The parameters for the query.</value>
        [JsonProperty(PropertyName = "parameters")]
        public IList<MessagingExtensionParameter> Parameters { get; private set; } = new List<MessagingExtensionParameter>();

        /// <summary>
        /// Gets or sets the query options.
        /// </summary>
        /// <value>The query options.</value>
        [JsonProperty(PropertyName = "queryOptions")]
        public MessagingExtensionQueryOptions QueryOptions { get; set; }

        /// <summary>
        /// Gets or sets state parameter passed back to the bot after
        /// authentication/configuration flow.
        /// </summary>
        /// <value>The state parameter passed back to the bot after authentication/configuration flow.</value>
        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }
    }
}
