// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Bot.Builder.Ai.LUIS
{
    /// <summary>
    /// Specifies the LUIS API versions.
    /// </summary>
    public enum LuisApiVersion
    {
        /// <summary>
        /// Version 1 of the LUIS API.
        /// </summary>
        [Obsolete]
        V1,

        /// <summary>
        /// Version 2 of the LUIS API.
        /// </summary>
        V2,
    }

    /// <summary>
    /// Specifies the usage of a LUIS model, including the parameters to use to get predictions from the model.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
    [Serializable]
    public class LuisModelAttribute : Attribute, ILuisModel, ILuisOptions, IEquatable<ILuisModel>
    {
        private readonly string modelID;
        private readonly string subscriptionKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuisModelAttribute"/> class.
        /// </summary>
        /// <param name="modelID">The LUIS model ID.</param>
        /// <param name="subscriptionKey">The LUIS subscription key.</param>
        /// <param name="apiVersion">The LUIS API version.</param>
        /// <param name="domain">The URL domain for the model.</param>
        /// <param name="threshold">The threshold for the top scoring intent.</param>
        public LuisModelAttribute(
            string modelID,
            string subscriptionKey,
            LuisApiVersion apiVersion = LuisApiVersion.V2,
            string domain = null,
            double threshold = 0.0d)
        {
            SetField.NotNull(out this.modelID, nameof(modelID), modelID);
            SetField.NotNull(out this.subscriptionKey, nameof(subscriptionKey), subscriptionKey);
            this.ApiVersion = apiVersion;
            this.Domain = domain;
            this.UriBase = UriFor(apiVersion, domain);
            this.Threshold = threshold;

            this.Log = true;
        }

        /// <summary>
        /// Gets the LUIS model's application ID.
        /// </summary>
        /// <value>
        /// The LUIS model's application ID.
        /// </value>
        public string ModelID => modelID;

        /// <summary>
        /// Gets the subscription key under which the model was published.
        /// </summary>
        /// <value>
        /// The subscription key under which the model was published, also known as the authoring key.
        /// </value>
        public string SubscriptionKey => subscriptionKey;

        /// <summary>
        /// Gets the URL domain for the model.
        /// </summary>
        /// <value>
        /// The URL domain for the model.
        /// </value>
        /// <remarks><code>null</code> signifies the default domain, which is api.projectoxford.ai for version 1 of the API
        /// and westus.api.cognitive.microsoft.com for version 2.</remarks>
        public string Domain { get; }

        /// <summary>
        /// Gets base URI for LUIS calls.
        /// </summary>
        /// <value>
        /// Base URI for LUIS calls.
        /// </value>
        public Uri UriBase { get; }

        /// <summary>
        /// Gets the version of the LUIS API to call.
        /// </summary>
        /// <value>
        /// The version of the LUIS API to call.
        /// </value>
        public LuisApiVersion ApiVersion { get; }

        /// <summary>
        /// Gets the threshold for top scoring intent.
        /// </summary>
        /// <value>
        /// The threshold for top scoring intent.
        /// </value>
        public double Threshold { get; }

        /// <summary>
        /// Gets or sets a value indicating whether to log the query.
        /// </summary>
        /// <value>
        /// Indicates whether to log the query. The default is true.
        /// </value>
        public bool Log
        {
            get { return Options.Log.Value; }
            set { Options.Log = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to enable spell checking.
        /// </summary>
        /// <value>
        /// Indicates whether to enable spell checking.
        /// </value>
        public bool SpellCheck
        {
            get { return Options.SpellCheck.Value; }
            set { Options.SpellCheck = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use the staging endpoint.
        /// </summary>
        /// <value>
        /// Indicates whether to use the staging endpoint.
        /// </value>
        public bool Staging
        {
            get { return Options.Staging.Value; }
            set { Options.Staging = value; }
        }

        /// <summary>
        /// Gets or sets the timezone offset for the location of the request in minutes.
        /// </summary>
        /// <value>
        /// The timezone offset for the location of the request in minutes.
        /// </value>
        public double TimezoneOffset
        {
            get { return Options.TimezoneOffset.Value; }
            set { Options.TimezoneOffset = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to return all intents instead of just the topscoring intent.
        /// </summary>
        /// <value>
        /// Indicates whether to return all intents instead of just the topscoring intent.
        /// </value>
        public bool Verbose
        {
            get { return Options.Verbose.Value; }
            set { Options.Verbose = value; }
        }

        /// <summary>
        /// Gets or sets the subscription key to use when enabling bing spell check.
        /// </summary>
        /// <value>
        /// The subscription key to use when enabling bing spell check.
        /// </value>
        public string BingSpellCheckSubscriptionKey
        {
            get { return Options.BingSpellCheckSubscriptionKey; }
            set { Options.BingSpellCheckSubscriptionKey = value;  }
        }

        bool? ILuisOptions.Log { get; set; }

        bool? ILuisOptions.SpellCheck { get; set; }

        bool? ILuisOptions.Staging { get; set; }

        double? ILuisOptions.TimezoneOffset { get; set; }

        bool? ILuisOptions.Verbose { get; set; }

        string ILuisOptions.BingSpellCheckSubscriptionKey { get; set; }

        private ILuisOptions Options => this;

        /// <summary>
        /// Gets the base URI for a version of the LUIS API and a URL domain.
        /// </summary>
        /// <param name="apiVersion">The LUIS API version.</param>
        /// <param name="domain">The URL domain for the model.</param>
        /// <returns>The base URI.</returns>
        public static Uri UriFor(LuisApiVersion apiVersion, string domain = null)
        {
            if (domain == null)
            {
                domain = apiVersion == LuisApiVersion.V2 ? "westus.api.cognitive.microsoft.com" : "api.projectoxford.ai/luis/v1/application";
            }

            return new Uri(apiVersion == LuisApiVersion.V2 ? $"https://{domain}/luis/v2.0/apps/" : $"https://api.projectoxford.ai/luis/v1/application");
        }

        /// <inheritdoc/>
        public bool Equals(ILuisModel other) => other != null
                && object.Equals(this.ModelID, other.ModelID)
                && object.Equals(this.SubscriptionKey, other.SubscriptionKey)
                && object.Equals(this.ApiVersion, other.ApiVersion)
                && object.Equals(this.UriBase, other.UriBase);

        /// <inheritdoc/>
        public override bool Equals(object other) => this.Equals(other as ILuisModel);

        /// <inheritdoc/>
        public override int GetHashCode() => ModelID.GetHashCode()
                ^ SubscriptionKey.GetHashCode()
                ^ UriBase.GetHashCode()
                ^ ApiVersion.GetHashCode();

        /// <summary>
        /// Applies the optional parameters from this object to a LUIS request.
        /// </summary>
        /// <param name="request">The LUIS request to modify.</param>
        /// <returns>The modified request.</returns>
        public LuisRequest ModifyRequest(LuisRequest request)
        {
            Options.Apply(request);
            return request;
        }
    }
}
