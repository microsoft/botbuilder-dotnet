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
    /// SharePoint GetPropertyPaneConfiguration response object.
    /// </summary>
    public class GetPropertyPaneConfigurationResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetPropertyPaneConfigurationResponse"/> class.
        /// </summary>
        public GetPropertyPaneConfigurationResponse()
        {
            // Do nothing
        }

        /// <summary>
        /// Gets or Sets the pages of type <see cref="PropertyPanePage"/>.
        /// </summary>
        /// <value>This value is the pages of the property pane.</value>
        [JsonProperty(PropertyName = "pages")]
        public IEnumerable<PropertyPanePage> Pages { get; set; }

        /// <summary>
        /// Gets or Sets the current page of type <see cref="int"/>.
        /// </summary>
        /// <value>This value is the current page of the property pane.</value>
        [JsonProperty(PropertyName = "currentPage")]
        public int CurrentPage { get; set; }

        /// <summary>
        /// Gets or Sets the loading indicator delay time of type <see cref="int"/>.
        /// </summary>
        /// <value>This value is the loading indicator delay time of the property pane.</value>
        [JsonProperty(PropertyName = "loadingIndicatorDelayTime")]
        public int LoadingIndicatorDelayTime { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether the loading indicator should be displayed on top of the property pane or not of type <see cref="bool"/>.
        /// </summary>
        /// <value>This value sets whether the loading indicator is shown for the property pane.</value>
        [JsonProperty(PropertyName = "showLoadingIndicator")]
        public bool ShowLoadingIndicator { get; set; }
    }
}
