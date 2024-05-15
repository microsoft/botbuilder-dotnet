// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using AdaptiveCards;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// SharePoint GetQuickView response object.
    /// </summary>
    public class QuickViewResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuickViewResponse"/> class.
        /// </summary>
        public QuickViewResponse()
        {
            // Do nothing
        }

        /// <summary>
        /// Gets or Sets data for the quick view.
        /// </summary>
        /// <value>This value is the data of the quick view response.</value>
        [JsonProperty(PropertyName = "data")]
        public object Data { get; set; }

        /// <summary>
        /// Gets or Sets data for the quick view template of type <see cref="AdaptiveCard"/>.
        /// </summary>
        /// <value>This value is the template of the quick view response.</value>
        [JsonProperty(PropertyName = "template")]
        public AdaptiveCard Template { get; set; }

        /// <summary>
        /// Gets or Sets view Id of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the view Id of the quick view response.</value>
        [JsonProperty(PropertyName = "viewId")]
        public string ViewId { get; set; }

        /// <summary>
        /// Gets or Sets title of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the title of the quick view response.</value>
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or Sets the external link parameters of type <see cref="ExternalLinkActionParameters"/>.
        /// </summary>
        /// <value>This value is the external link parameters of the quick view response.</value>
        [JsonProperty(PropertyName = "externalLink")]
        public ExternalLinkActionParameters ExternalLink { get; set; }

        /// <summary>
        /// Gets or Sets focus parameters of type <see cref="FocusParameters"/>.
        /// </summary>
        /// <value>This value is the focus parameters of the quick view response.</value>
        [JsonProperty(PropertyName = "focusParameters")]
        public FocusParameters FocusParameters { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether the client to trigger a single sign on flow.
        /// </summary>
        /// <value>
        /// True if single sign on flow should be started.
        /// </value>
        [JsonProperty(PropertyName = "requiresSso")]
        public bool RequiresSso { get; set; }

        /// <summary>
        /// Gets or Sets a value which tells the client what view to load after SSO is complete.
        /// </summary>
        /// <value>
        /// ViewId to load from client after the SSO flow completes.
        /// </value>
        [JsonProperty(PropertyName = "postSsoViewId")]
        public string PostSsoViewId { get; set; }
    }
}
