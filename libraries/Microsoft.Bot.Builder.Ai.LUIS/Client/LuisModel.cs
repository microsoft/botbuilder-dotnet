// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Bot.Builder.Ai.LUIS
{
    /// <summary>
    /// Luis api version.
    /// </summary>
    public enum LuisApiVersion
    {
        /// <summary>
        /// Represents the version 1 of the LUIS API.
        /// </summary>
        [Obsolete]
        V1,

        /// <summary>
        /// Represents the version 2 of the LUIS API.
        /// </summary>
        V2,
    }

    /// <summary>
    /// A mockable interface for the LUIS model.
    /// </summary>
    public interface ILuisModel
    {
        /// <summary>
        /// Gets the GUID for the LUIS model.
        /// </summary>
        /// <value>
        /// The GUID for the LUIS model.
        /// </value>
        string ModelID { get; }

        /// <summary>
        /// Gets the subscription key for LUIS.
        /// </summary>
        /// <value>
        /// The subscription key for LUIS.
        /// </value>
        string SubscriptionKey { get; }

        /// <summary>
        /// Gets base URI for LUIS calls.
        /// </summary>
        /// <value>
        /// Base URI for LUIS calls.
        /// </value>
        Uri UriBase { get; }

        /// <summary>
        /// Gets version of the LUIS API to call.
        /// </summary>
        /// <value>
        /// Version of the LUIS API to call.
        /// </value>
        LuisApiVersion ApiVersion { get; }

        /// <summary>
        /// Gets threshold for top scoring intent.
        /// </summary>
        /// <value>
        /// Threshold for top scoring intent.
        /// </value>
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
        private readonly string subscriptionKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuisModelAttribute"/> class.
        /// </summary>
        /// <param name="modelID">The LUIS model ID.</param>
        /// <param name="subscriptionKey">The LUIS subscription key.</param>
        /// <param name="apiVersion">The LUIS API version.</param>
        /// <param name="domain">Domain where LUIS model is located.</param>
        /// <param name="threshold">Threshold for the top scoring intent.</param>
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
        /// Gets the GUID for the LUIS model.
        /// </summary>
        /// <value>
        /// The GUID for the LUIS model.
        /// </value>
        public string ModelID => modelID;

        /// <summary>
        /// Gets the subscription key for LUIS.
        /// </summary>
        /// <value>
        /// The subscription key for LUIS.
        /// </value>
        public string SubscriptionKey => subscriptionKey;

        /// <summary>
        /// Gets domain where LUIS application is located.
        /// </summary>
        /// <remarks>Null means default which is api.projectoxford.ai for V1 API and westus.api.cognitive.microsoft.com for V2 api.</remarks>
        /// <value>
        /// Domain where LUIS application is located.
        /// </value>
        public string Domain { get; }

        /// <summary>
        /// Gets base URI for LUIS calls.
        /// </summary>
        /// <value>
        /// Base URI for LUIS calls.
        /// </value>
        public Uri UriBase { get; }

        /// <summary>
        /// Gets version of the LUIS API to call.
        /// </summary>
        /// <value>
        /// Version of the LUIS API to call.
        /// </value>
        public LuisApiVersion ApiVersion { get; }

        /// <summary>
        /// Gets threshold for top scoring intent.
        /// </summary>
        /// <value>
        /// Threshold for top scoring intent.
        /// </value>
        public double Threshold { get; }

        /// <summary>
        /// Gets or sets a value indicating whether if logging of queries to LUIS is allowed.
        /// </summary>
        /// <value>
        /// Indicates if logging of queries to LUIS is allowed.
        /// </value>
        public bool Log
        {
            get { return Options.Log.Value; }
            set { Options.Log = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether if spell checking is enabled.
        /// </summary>
        /// <value>
        /// Indicates if spell checking is enabled.</placeholder>
        /// </value>
        public bool SpellCheck
        {
            get { return Options.SpellCheck.Value; }
            set { Options.SpellCheck = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether if the staging endpoint is used..
        /// </summary>
        /// <value>
        /// Indicates if the staging endpoint is used.
        /// </value>
        public bool Staging
        {
            get { return Options.Staging.Value; }
            set { Options.Staging = value; }
        }

        /// <summary>
        /// Gets or sets the time zone offset.
        /// </summary>
        /// <value>
        /// The time zone offset.
        /// </value>
        public double TimezoneOffset
        {
            get { return Options.TimezoneOffset.Value; }
            set { Options.TimezoneOffset = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the verbose flag is used.
        /// </summary>
        /// <value>
        /// Indicates if the verbose flag is used.
        /// </value>
        public bool Verbose
        {
            get { return Options.Verbose.Value; }
            set { Options.Verbose = value; }
        }

        /// <summary>
        /// Gets or sets the Bing Spell Check subscription key.
        /// </summary>
        /// <value>
        /// The Bing Spell Check subscription key.
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

        public static Uri UriFor(LuisApiVersion apiVersion, string domain = null)
        {
            if (domain == null)
            {
                domain = apiVersion == LuisApiVersion.V2 ? "westus.api.cognitive.microsoft.com" : "api.projectoxford.ai/luis/v1/application";
            }

            return new Uri(apiVersion == LuisApiVersion.V2 ? $"https://{domain}/luis/v2.0/apps/" : $"https://api.projectoxford.ai/luis/v1/application");
        }

        public bool Equals(ILuisModel other) => other != null
                && object.Equals(this.ModelID, other.ModelID)
                && object.Equals(this.SubscriptionKey, other.SubscriptionKey)
                && object.Equals(this.ApiVersion, other.ApiVersion)
                && object.Equals(this.UriBase, other.UriBase);

        public override bool Equals(object other) => this.Equals(other as ILuisModel);

        public override int GetHashCode() => ModelID.GetHashCode()
                ^ SubscriptionKey.GetHashCode()
                ^ UriBase.GetHashCode()
                ^ ApiVersion.GetHashCode();

        public LuisRequest ModifyRequest(LuisRequest request)
        {
            Options.Apply(request);
            return request;
        }
    }
}
