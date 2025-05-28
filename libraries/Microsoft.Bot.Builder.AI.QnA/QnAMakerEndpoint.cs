// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.AI.QnA.Models;
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
        [Obsolete("This constructor is obsolete, the QnAMakerService class is obsolete and will be removed in a future version of the framework.")]
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
        /// Gets or sets the QnAServiceType to query QnAMaker or Custom Question Answering Knowledge Base.
        /// </summary>
        /// <value>
        /// Valid value <see cref="ServiceType.Language"/> for Language Service, <see cref="ServiceType.QnAMaker"/> for QnAMaker.
        /// </value>
        [JsonProperty("qnAServiceType")]
        public ServiceType QnAServiceType { get; set; } = ServiceType.QnAMaker;

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

        /// <summary>
        /// Gets or sets the ClientId of the Managed Identity resource. Access control (IAM) role `Cognitive Services User` must be assigned in the Language resource to the Managed Identity resource.
        /// </summary>
        /// <value>
        /// The ClientId of the Managed Identity resource.
        /// </value>
        [JsonProperty("managedIdentityClientId")]
        public string ManagedIdentityClientId { get; set; }
    }
}
