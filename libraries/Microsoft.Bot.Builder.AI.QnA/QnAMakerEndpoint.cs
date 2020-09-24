// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Configuration;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// Defines an endpoint used to connect to a QnA Maker Knowledge base.
    /// </summary>
    public class QnAMakerEndpoint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QnAMakerEndpoint"/> class.
        /// </summary>
        public QnAMakerEndpoint()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QnAMakerEndpoint"/> class.
        /// </summary>
        /// <param name="service">QnA service details from configuration.</param>
        public QnAMakerEndpoint(QnAMakerService service)
        {
            KnowledgeBaseId = service.KbId;
            EndpointKey = service.EndpointKey;
            Host = service.Hostname;
        }

        /// <summary>
        /// Gets or sets the knowledge base ID.
        /// </summary>
        /// <value>
        /// The knowledge base ID.
        /// </value>
        [JsonProperty("knowledgeBaseId")]
        public string KnowledgeBaseId { get; set; }

        /// <summary>
        /// Gets or sets the endpoint key for the knowledge base.
        /// </summary>
        /// <value>
        /// The endpoint key for the knowledge base.
        /// </value>
        [JsonProperty("endpointKey")]
        public string EndpointKey { get; set; }

        /// <summary>
        /// Gets or sets the host path. For example "https://westus.api.cognitive.microsoft.com/qnamaker/v2.0".
        /// </summary>
        /// <value>
        /// The host path.
        /// </value>
        [JsonProperty("host")]
        public string Host { get; set; }
    }
}
