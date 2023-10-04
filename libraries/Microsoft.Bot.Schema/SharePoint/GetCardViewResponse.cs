// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// SharePoint GetCardView response object.
    /// </summary>
    /// <typeparam name="TData">Type for data field.</typeparam>
    /// <typeparam name="TAceData">Type for ACE data field.</typeparam>
    public class GetCardViewResponse<TData, TAceData> : BaseResponse<TData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetCardViewResponse{TData, TAceData}"/> class.
        /// </summary>
        /// <param name="schemaVersion">Schema version to be used.</param>
        public GetCardViewResponse(string schemaVersion)
            : base(schemaVersion)
        {
        }

        /// <summary>
        /// Gets or sets open-ended AceData for the card view.
        /// </summary>
        /// <value>This value is the ace data of the card view response.</value>
        [JsonProperty(PropertyName = "aceData")]
        public object AceData { get; set; }
    }
}
