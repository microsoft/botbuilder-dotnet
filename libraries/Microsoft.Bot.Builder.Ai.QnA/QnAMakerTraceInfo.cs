// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.QnA
{

    /// <summary>
    /// This class represents all the trace info that we collect from the LUIS Recognizer Middleware
    /// </summary>
    public class QnAMakerTraceInfo
    {
        [JsonProperty("queryResults")]
        public QueryResult[] QueryResults { set; get; }

        [JsonProperty("qnaMakerOptions")]
        public QnAMakerOptions QnAMakerOptions { set; get; }
    }
}
