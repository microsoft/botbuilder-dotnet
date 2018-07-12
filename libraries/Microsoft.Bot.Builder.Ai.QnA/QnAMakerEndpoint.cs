// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Ai.QnA
{
    /// <summary>
    /// Defines an endpoint used to connect to a QnA Maker Knowledge base.
    /// </summary>
    public class QnAMakerEndpoint
    {
        /// <summary>
        /// Gets or sets the knowledge base ID.
        /// </summary>
        /// <value>
        /// The knowledge base ID.
        /// </value>
        public string KnowledgeBaseId { get; set; }

        /// <summary>
        /// Gets or sets the endpoint key for the knowledge base.
        /// </summary>
        /// <value>
        /// The endpoint key for the knowledge base.
        /// </value>
        public string EndpointKey { get; set; }

        /// <summary>
        /// Gets or sets the host path. For example "https://westus.api.cognitive.microsoft.com/qnamaker/v2.0".
        /// </summary>
        /// <value>
        /// The host path.
        /// </value>
        public string Host { get; set; }
    }
}
