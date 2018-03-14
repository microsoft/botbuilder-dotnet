using System;
using Microsoft.Cognitive.LUIS;

namespace Microsoft.Bot.Builder.LUIS
{
    public class LuisModel : ILuisModel
    {
        public LuisModel(string modelId, string subscriptionKey, Uri uriBase, LuisApiVersion apiVersion = LuisApiVersion.V2)
        {
            if(string.IsNullOrEmpty(modelId))
                throw new ArgumentNullException(nameof(modelId));

            if (string.IsNullOrEmpty(subscriptionKey))
                throw new ArgumentNullException(nameof(subscriptionKey));

            if (uriBase == null)
                throw new ArgumentNullException(nameof(uriBase));

            ModelID = modelId;
            SubscriptionKey = subscriptionKey;
            UriBase = uriBase;
            ApiVersion = apiVersion;
        }

        public string ModelID { get; }

        public string SubscriptionKey { get; }

        public Uri UriBase { get; }

        public LuisApiVersion ApiVersion { get; }

        public double Threshold => 0.0d;

        public LuisRequest ModifyRequest(LuisRequest request)
        {
            return request;
        }
    }
}
