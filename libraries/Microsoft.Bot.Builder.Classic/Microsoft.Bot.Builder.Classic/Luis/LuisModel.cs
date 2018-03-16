// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK GitHub:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using Microsoft.Bot.Builder.Classic.Internals.Fibers;

namespace Microsoft.Bot.Builder.Classic.Luis
{
    /// <summary>
    /// Luis api version.
    /// </summary>
    public enum LuisApiVersion
    {
        [Obsolete]
        V1,
        V2
    }

    /// <summary>
    /// A mockable interface for the LUIS model.
    /// </summary>
    public interface ILuisModel
    {
        /// <summary>
        /// The LUIS model ID.
        /// </summary>
        string ModelID { get; }

        /// <summary>
        /// The LUIS subscription key.
        /// </summary>
        string SubscriptionKey { get; }

        /// <summary>
        /// The base Uri for accessing LUIS.
        /// </summary>
        Uri UriBase { get; }

        /// <summary>
        /// Luis Api Version.
        /// </summary>
        LuisApiVersion ApiVersion { get; }

        /// <summary>
        /// Threshold for top scoring intent
        /// </summary>
        double Threshold { get; }
        
        /// <summary>
        /// Modify a Luis request to specify query parameters like spelling or logging.
        /// </summary>
        /// <param name="request">Request so far.</param>
        /// <returns>Modified request.</returns>
        LuisRequest ModifyRequest(LuisRequest request);
    }

    /// <summary>
    /// The LUIS model information.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
    [Serializable]
    public class LuisModelAttribute : Attribute, ILuisModel, ILuisOptions, IEquatable<ILuisModel>
    {
        private readonly string modelID;
        /// <summary>
        /// The GUID for the LUIS model.
        /// </summary>
        public string ModelID => modelID;

        private readonly string subscriptionKey;
        /// <summary>
        /// The subscription key for LUIS.
        /// </summary>
        public string SubscriptionKey => subscriptionKey;

        private readonly string domain;
        /// <summary>
        /// Domain where LUIS application is located.
        /// </summary>
        /// <remarks>Null means default which is api.projectoxford.ai for V1 API and westus.api.cognitive.microsoft.com for V2 api.</remarks>
        public string Domain => domain;

        private readonly Uri uriBase;
        /// <summary>
        /// Base URI for LUIS calls.
        /// </summary>
        public Uri UriBase => uriBase;

        private readonly LuisApiVersion apiVersion;
        /// <summary>
        /// Version of query API to call.
        /// </summary>
        public LuisApiVersion ApiVersion => apiVersion;

	    private readonly double threshold;
        /// <summary>
        /// Threshold for top scoring intent
        /// </summary>
        public double Threshold => threshold;

        private ILuisOptions Options => (ILuisOptions)this;

        /// <summary>
        /// Indicates if logging of queries to LUIS is allowed.
        /// </summary>
        public bool Log { get { return Options.Log.Value; } set { Options.Log = value; } }

        /// <summary>
        /// Turn on spell checking.
        /// </summary>
        public bool SpellCheck { get { return Options.SpellCheck.Value; } set { Options.SpellCheck = value; } }

        /// <summary>
        /// Use the staging endpoint.
        /// </summary>
        public bool Staging { get { return Options.Staging.Value; } set { Options.Staging = value; } }

        /// <summary>
        /// The time zone offset.
        /// </summary>
        public double TimezoneOffset { get { return Options.TimezoneOffset.Value; } set { Options.TimezoneOffset = value; } }

        /// <summary>
        /// The verbose flag.
        /// </summary>
        public bool Verbose { get { return Options.Verbose.Value; } set { Options.Verbose = value; } }

        /// <summary>
        /// The Bing Spell Check subscription key.
        /// </summary>
        public string BingSpellCheckSubscriptionKey { get { return Options.BingSpellCheckSubscriptionKey; } set { Options.BingSpellCheckSubscriptionKey = value;  } }

        bool? ILuisOptions.Log { get; set; }
        bool? ILuisOptions.SpellCheck { get; set; }
        bool? ILuisOptions.Staging { get; set; }
        double? ILuisOptions.TimezoneOffset { get; set; }
        bool? ILuisOptions.Verbose { get; set; }
        string ILuisOptions.BingSpellCheckSubscriptionKey { get; set; }

        public static Uri UriFor(LuisApiVersion apiVersion, string domain = null)
        {
            if (domain == null)
            {
                domain = apiVersion == LuisApiVersion.V2 ? "westus.api.cognitive.microsoft.com" : "api.projectoxford.ai/luis/v1/application";
            }

            return new Uri(apiVersion == LuisApiVersion.V2 ? $"https://{domain}/luis/v2.0/apps/" : $"https://api.projectoxford.ai/luis/v1/application");
        }

        /// <summary>
        /// Construct the LUIS model information.
        /// </summary>
        /// <param name="modelID">The LUIS model ID.</param>
        /// <param name="subscriptionKey">The LUIS subscription key.</param>
        /// <param name="apiVersion">The LUIS API version.</param>
        /// <param name="domain">Domain where LUIS model is located.</param>
	    /// <param name="threshold">Threshold for the top scoring intent.</param>
	    public LuisModelAttribute(string modelID, string subscriptionKey,
            LuisApiVersion apiVersion = LuisApiVersion.V2, string domain = null, double threshold = 0.0d)
        {
            SetField.NotNull(out this.modelID, nameof(modelID), modelID);
            SetField.NotNull(out this.subscriptionKey, nameof(subscriptionKey), subscriptionKey);
            this.apiVersion = apiVersion;
            this.domain = domain;
            this.uriBase = UriFor(apiVersion, domain);
            this.threshold = threshold;

            this.Log = true;
        }

        public bool Equals(ILuisModel other)
        {
            return other != null
                && object.Equals(this.ModelID, other.ModelID)
                && object.Equals(this.SubscriptionKey, other.SubscriptionKey)
                && object.Equals(this.ApiVersion, other.ApiVersion)
                && object.Equals(this.UriBase, other.UriBase)
                ;
        }

        public override bool Equals(object other)
        {
            return this.Equals(other as ILuisModel);
        }

        public override int GetHashCode()
        {
            return ModelID.GetHashCode()
                ^ SubscriptionKey.GetHashCode()
                ^ UriBase.GetHashCode()
                ^ ApiVersion.GetHashCode();
        }

        public LuisRequest ModifyRequest(LuisRequest request)
        {
            Options.Apply(request);
            return request;
        }
    }
}
