// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Configuration
{
    [Obsolete("This class is deprecated.  See https://aka.ms/bot-file-basics for more information.", false)]
    public class ServiceTypes
    {
        public const string AppInsights = "appInsights";
        public const string BlobStorage = "blob";
        public const string CosmosDB = "cosmosdb";
        public const string Bot = "abs";
        public const string Generic = "generic";
        public const string Dispatch = "dispatch";
        public const string Endpoint = "endpoint";
        public const string File = "file";
        public const string Luis = "luis";
        public const string QnA = "qna";
    }
}
