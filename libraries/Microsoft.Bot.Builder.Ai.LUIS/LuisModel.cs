// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder.Ai.LUIS
{
    public class LuisModel : ILuisModel
    {
        public LuisModel(string modelId, string subscriptionKey, Uri uriBase, LuisApiVersion apiVersion = LuisApiVersion.V2)
        {
            if(string.IsNullOrEmpty(modelId))
                throw new ArgumentNullException(nameof(modelId));

            if (string.IsNullOrEmpty(subscriptionKey))
                throw new ArgumentNullException(nameof(subscriptionKey));

            ModelID = modelId;
            SubscriptionKey = subscriptionKey;
            UriBase = uriBase ?? throw new ArgumentNullException(nameof(uriBase));
            ApiVersion = apiVersion;
        }

        public string ModelID { get; set; }

        public string SubscriptionKey { get; set; }

        public Uri UriBase { get; set; }

        public LuisApiVersion ApiVersion { get; set; }

        public double Threshold => 0.0d;

        public LuisRequest ModifyRequest(LuisRequest request)
        {
            return request;
        }
    }
}
