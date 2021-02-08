// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Net;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Teams.Actions
{
    /// <summary>
    /// Base dialog for Teams Invoke Responses having a CacheInfo property.
    /// </summary>
    public abstract class BaseTeamsCacheInfoResponseDialog : Dialog
    {
        /// <summary>
        /// Gets or sets an optional expression which if is true will disable this action.
        /// </summary>
        /// <example>
        /// "user.age > 18".
        /// </example>
        /// <value>
        /// A boolean expression. 
        /// </value>
        [JsonProperty("disabled")]
        public BoolExpression Disabled { get; set; }

        /// <summary>
        /// Gets or sets config CacheType.
        /// </summary>
        /// <value>
        /// "cache" or "no_cache".
        /// </value>
        [JsonProperty("cacheType")]
        public StringExpression CacheType { get; set; }

        /// <summary>
        /// Gets or sets cache duration in seconds for which the cached object should remain in the cache.
        /// </summary>
        /// <value>
        /// Duration the result should be cached in seconds.
        /// </value>
        [JsonProperty("cacheDuration")]
        public IntExpression CacheDuration { get; set; }
        
        /// <summary>
        /// Create an InvokeResponse activity with the specified body.
        /// </summary>
        /// <param name="body">The body to return in the InvokeResponse.</param>
        /// <param name="statusCode"><see cref="HttpStatusCode"/> for the InvokeResponse.  Default is HttpStatusCode.OK.</param>
        /// <returns>An Activity of type InvokeResponse, containing a Value of InvokeResponse.</returns>
        protected static Activity CreateInvokeResponseActivity(object body, int statusCode = (int)HttpStatusCode.OK)
        {
            return new Activity
            {
                Value = new InvokeResponse
                {
                    Status = statusCode,
                    Body = body
                },
                Type = ActivityTypesEx.InvokeResponse
            };
        }

        protected Activity CreateMessagingExtensionInvokeResponseActivity(DialogContext dc, MessagingExtensionResult result)
        {
            switch (dc.Context.Activity.Name)
            {
                case "composeExtension/queryLink":
                case "composeExtension/query":
                case "composeExtension/selectItem":
                case "composeExtension/querySettingUrl":
                    return CreateInvokeResponseActivity(new MessagingExtensionResponse() { ComposeExtension = result, CacheInfo = GetCacheInfo(dc) });
                case "composeExtension/submitAction":
                case "composeExtension/fetchTask":
                    return CreateInvokeResponseActivity(new MessagingExtensionActionResponse() { ComposeExtension = result, CacheInfo = GetCacheInfo(dc) });
                default:
                    throw new InvalidOperationException($"GetMessagingExtensionResponse Invalid Activity.Name: {dc.Context.Activity.Name}");
            }
        }

        /// <summary>
        /// Retrieve a Cache Info object from the CacheType and CacheDuration, if present.
        /// </summary>
        /// <param name="dc">Dialog Context to use for retrieving CacheType and CacheDuration from state.</param>
        /// <returns>A <see cref="CacheInfo"/> object if <see cref="CacheDuration"/> 
        /// and <see cref="CacheType"/> resolve to valid values.</returns>
        protected CacheInfo GetCacheInfo(DialogContext dc)
        {
            if (CacheType != null && CacheDuration != null)
            {
                var cacheType = CacheType.GetValueOrNull(dc.State);
                var cacheDuration = CacheDuration.GetValueOrNull(dc.State);
                if (cacheDuration.HasValue && cacheDuration.Value > 0 && !string.IsNullOrEmpty(cacheType))
                {
                    // Valid ranges for CacheDuration are 60 < > 2592000
                    cacheDuration = Math.Min(Math.Max(60, cacheDuration.Value), 2592000);
                    return new CacheInfo(cacheType: cacheType, cacheDuration: cacheDuration.Value);
                }
            }

            return null;
        }
    }
}
