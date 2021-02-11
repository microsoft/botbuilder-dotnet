// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Configuration
{
    /// <summary>
    /// Constants for Azure service types.
    /// </summary>
    [Obsolete("This class is deprecated.  See https://aka.ms/bot-file-basics for more information.", false)]
#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable (this class is obsolete, we won't fix it)
    public class ServiceTypes
#pragma warning restore CA1052 // Static holder types should be Static or NotInheritable
    {
        /// <summary>
        /// Application Insights.
        /// </summary>
        public const string AppInsights = "appInsights";

        /// <summary>
        /// Blob Storage.
        /// </summary>
        public const string BlobStorage = "blob";

        /// <summary>
        /// Cosmos DB.
        /// </summary>
        public const string CosmosDB = "cosmosdb";

        /// <summary>
        /// Azure Bot Service.
        /// </summary>
        public const string Bot = "abs";

        /// <summary>
        /// Generic service.
        /// </summary>
        public const string Generic = "generic";

        /// <summary>
        /// Dispatch.
        /// </summary>
        public const string Dispatch = "dispatch";

        /// <summary>
        /// Bot Endpoint. 
        /// </summary>
        public const string Endpoint = "endpoint";

        /// <summary>
        /// File service.
        /// </summary>
        public const string File = "file";

        /// <summary>
        /// LUIS Cognitive Service.
        /// </summary>
        public const string Luis = "luis";

        /// <summary>
        /// QnA Maker Cognitive Service.
        /// </summary>
        public const string QnA = "qna";
    }
}
