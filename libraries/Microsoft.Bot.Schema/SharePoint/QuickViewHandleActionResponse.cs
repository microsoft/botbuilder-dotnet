// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// Adaptive Card Extension Client-side action response to render quick view.
    /// </summary>
    public class QuickViewHandleActionResponse : BaseHandleActionResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuickViewHandleActionResponse"/> class.
        /// </summary>
        public QuickViewHandleActionResponse() 
            : base(ViewResponseType.QuickView)
        {
            // Do nothing
        }

        /// <summary>
        /// Gets or sets card view render arguments.
        /// </summary>
        /// <value>Card view render arguments.</value>
        [JsonProperty(PropertyName = "renderArguments")]
        public new QuickViewResponse RenderArguments { get; set; }
    }
}
