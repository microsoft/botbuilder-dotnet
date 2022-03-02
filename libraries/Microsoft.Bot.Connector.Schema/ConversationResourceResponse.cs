// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary>A response containing a resource.</summary>
    public partial class ConversationResourceResponse
    {
        /// <summary>Initializes a new instance of the <see cref="ConversationResourceResponse"/> class.</summary>
        public ConversationResourceResponse()
        {
            CustomInit();
        }

        /// <summary>Initializes a new instance of the <see cref="ConversationResourceResponse"/> class.</summary>
        /// <param name="activityId">ID of the Activity (if sent).</param>
        /// <param name="serviceUrl">Service endpoint where operations concerning the conversation may be performed.</param>
        /// <param name="id">Id of the resource.</param>
        public ConversationResourceResponse(string activityId = default, string serviceUrl = default, string id = default)
        {
            ActivityId = activityId;
            ServiceUrl = serviceUrl;
            Id = id;
            CustomInit();
        }

        /// <summary>Gets or sets ID of the Activity (if sent).</summary>
        /// <value>The activity ID.</value>
        [JsonPropertyName("activityId")]
        public string ActivityId { get; set; }

        /// <summary>Gets or sets service endpoint where operations concerning the conversation may be performed.</summary>
        /// <value>The service URL.</value>
        [JsonPropertyName("serviceUrl")]
#pragma warning disable CA1056 // Uri properties should not be strings
        public string ServiceUrl { get; set; }
#pragma warning restore CA1056 // Uri properties should not be strings

        /// <summary>Gets or sets id of the resource.</summary>
        /// <value>The resource ID.</value>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>An initialization method that performs custom operations like setting defaults.</summary>
        partial void CustomInit();
    }
}
