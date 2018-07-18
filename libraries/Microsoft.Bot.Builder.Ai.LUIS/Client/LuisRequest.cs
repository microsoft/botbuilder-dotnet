// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.Rest;

namespace Microsoft.Bot.Builder.Ai.LUIS
{
    /// <summary>
    /// Represents a LUIS query, including the parameters to use to get predictions from the model.
    /// </summary>
    public sealed class LuisRequest : ILuisOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LuisRequest"/> class.
        /// </summary>
        public LuisRequest()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LuisRequest"/> class.
        /// </summary>
        /// <param name="query">The query text to get predictions for.</param>
        public LuisRequest(string query)
        {
            this.Query = query;
            this.Log = true;
        }

        /// <summary>
        /// Gets or sets the query text to get predictions for.
        /// </summary>
        /// <value>
        /// The query text to get predictions for.
        /// </value>
        public string Query { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to log the query.
        /// </summary>
        /// <value>
        /// Indicates whether to log the query. The default is true.
        /// </value>
        public bool? Log { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable spell checking.
        /// </summary>
        /// <value>
        /// Indicates whether to enable spell checking.
        /// </value>
        public bool? SpellCheck { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use the staging endpoint.
        /// </summary>
        /// <value>
        /// Indicates whether to use the staging endpoint.
        /// </value>
        public bool? Staging { get; set; }

        /// <summary>
        /// Gets or sets the timezone offset for the location of the request in minutes.
        /// </summary>
        /// <value>
        /// The timezone offset for the location of the request in minutes.
        /// </value>
        public double? TimezoneOffset { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to return all intents instead of just the topscoring intent.
        /// </summary>
        /// <value>
        /// Indicates whether to return all intents instead of just the topscoring intent.
        /// </value>
        public bool? Verbose { get; set; }

        /// <summary>
        /// Gets or sets the subscription key to use when enabling bing spell check.
        /// </summary>
        /// <value>
        /// The subscription key to use when enabling bing spell check.
        /// </value>
        public string BingSpellCheckSubscriptionKey { get; set; }

        /// <summary>
        /// Gets or sets any extra query parameters for the URL.
        /// </summary>
        /// <value>
        /// Extra query parameters for the URL.
        /// </value>
        public string ExtraParameters { get; set; }

        /// <summary>
        /// Gets or sets the context ID.
        /// </summary>
        /// <value>
        /// The context ID.
        /// </value>
        [Obsolete("Action binding in LUIS should be replaced with code.")]
        public string ContextId { get; set; }

        /// <summary>
        /// Gets or sets force setting the parameter when using action binding.
        /// </summary>
        /// <value>
        /// Force setting the parameter when using action binding.
        /// </value>
        [Obsolete("Action binding in LUIS should be replaced with code.")]
        public string ForceSet { get; set; }

        /// <summary>
        /// Builds a URI to use to get a prediction from the LUIS model.
        /// </summary>
        /// <param name="model"> The Luis model.</param>
        /// <returns> The request Uri.</returns>
        public Uri BuildUri(ILuisModel model)
        {
            if (model.ModelID == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "id");
            }

            if (model.SubscriptionKey == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "subscriptionKey");
            }

            var queryParameters = new List<string>
            {
                $"subscription-key={Uri.EscapeDataString(model.SubscriptionKey)}",
                $"q={Uri.EscapeDataString(Query)}",
            };
            UriBuilder builder;

            var id = Uri.EscapeDataString(model.ModelID);
            switch (model.ApiVersion)
            {
#pragma warning disable CS0612
                case LuisApiVersion.V1:
                    builder = new UriBuilder(model.UriBase);
                    queryParameters.Add($"id={id}");
                    break;
#pragma warning restore CS0612
                case LuisApiVersion.V2:
                    // v2.0 have the model as path parameter
                    builder = new UriBuilder(new Uri(model.UriBase, id));
                    break;
                default:
                    throw new ArgumentException($"{model.ApiVersion} is not a valid Luis api version.");
            }

            if (Log != null)
            {
                queryParameters.Add($"log={Uri.EscapeDataString(Convert.ToString(Log))}");
            }

            if (SpellCheck != null)
            {
                queryParameters.Add($"spellCheck={Uri.EscapeDataString(Convert.ToString(SpellCheck))}");
            }

            if (Staging != null)
            {
                queryParameters.Add($"staging={Uri.EscapeDataString(Convert.ToString(Staging))}");
            }

            if (TimezoneOffset != null)
            {
                queryParameters.Add($"timezoneOffset={Uri.EscapeDataString(Convert.ToString(TimezoneOffset))}");
            }

            if (Verbose != null)
            {
                queryParameters.Add($"verbose={Uri.EscapeDataString(Convert.ToString(Verbose))}");
            }

            if (!string.IsNullOrWhiteSpace(BingSpellCheckSubscriptionKey))
            {
                queryParameters.Add($"bing-spell-check-subscription-key={Uri.EscapeDataString(BingSpellCheckSubscriptionKey)}");
            }
#pragma warning disable CS0618
            if (ContextId != null)
            {
                queryParameters.Add($"contextId={Uri.EscapeDataString(ContextId)}");
            }

            if (ForceSet != null)
            {
                queryParameters.Add($"forceSet={Uri.EscapeDataString(ForceSet)}");
            }
#pragma warning restore CS0618
            if (ExtraParameters != null)
            {
                queryParameters.Add(ExtraParameters);
            }

            builder.Query = string.Join("&", queryParameters);
            return builder.Uri;
        }
    }
}
