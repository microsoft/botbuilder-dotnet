// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema.Teams
{
    /// <summary>
    /// Messaging extension query.
    /// </summary>
    public class MessagingExtensionQuery
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingExtensionQuery"/> class.
        /// </summary>
        public MessagingExtensionQuery()
        {
            CustomInit();
        }

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
            Parameters = parameters;
            QueryOptions = queryOptions;
            State = state;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets ID of the command assigned by Bot.
        /// </summary>
        /// <value>The ID of the command assigned by the Bot.</value>
        [JsonPropertyName("commandId")]
        public string CommandId { get; set; }

        /// <summary>
        /// Gets or sets parameters for the query.
        /// </summary>
        /// <value>The parameters for the query.</value>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Property setter is required for the collection to be deserialized")]
        [JsonPropertyName("parameters")]
        public IList<MessagingExtensionParameter> Parameters { get; set; }

        /// <summary>
        /// Gets or sets the query options.
        /// </summary>
        /// <value>The query options.</value>
        [JsonPropertyName("queryOptions")]
        public MessagingExtensionQueryOptions QueryOptions { get; set; }

        /// <summary>
        /// Gets or sets state parameter passed back to the bot after
        /// authentication/configuration flow.
        /// </summary>
        /// <value>The state parameter passed back to the bot after authentication/configuration flow.</value>
        [JsonPropertyName("state")]
        public string State { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
        }
    }
}
