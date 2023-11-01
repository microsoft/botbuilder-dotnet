// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// Adaptive Card Extension View response type.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ViewResponseType
    {
        /// <summary>
        /// Render card view.
        /// </summary>
        Card,

        /// <summary>
        /// Render quick view.
        /// </summary>
        QuickView,

        /// <summary>
        /// No operation.
        /// </summary>
        NoOp
    }

    /// <summary>
    /// Response returned when handling a client-side action on an Adaptive Card Extension.
    /// </summary>
    public abstract class BaseHandleActionResponse
    {
        /// <summary>
        /// Gets the response type.
        /// </summary>
        /// <value>Response type.</value>
        [JsonProperty(PropertyName = "responseType")]
        public abstract ViewResponseType ResponseType { get; }

        /// <summary>
        /// Gets or sets render arguments.
        /// </summary>
        /// <value>Render arguments.</value>
        [JsonProperty(PropertyName = "renderArguments")]
        public object RenderArguments { get; set; }
    }
}
