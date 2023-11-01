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
    /// Adaptive Card Extension search footer component.
    /// </summary>
    public class CardSearchFooterComponent : BaseCardComponent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CardSearchFooterComponent"/> class.
        /// </summary>
        public CardSearchFooterComponent()
            : base(CardComponentName.SearchFooter)
        {
            // this.ComponentName = CardComponentName.SearchFooter;
        }

        /// <summary>
        /// Gets or sets the title to display.
        /// </summary>
        /// <value>Title value to display in the search footer.</value>
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets url to the image to use, should be a square aspect ratio and big enough to fit in the image area.
        /// </summary>
        /// <value>Image Url to display in the footer.</value>
        [JsonProperty(PropertyName = "imageUrl")]
        public Uri ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the initials to display in the image area when there is no image.
        /// </summary>
        /// <value>Initials to display in the image area when there is no image.</value>
        [JsonProperty(PropertyName = "imageInitials")]
        public string ImageInitials { get; set; }

        /// <summary>
        /// Gets or sets the primary text to display. For example, name of the person for people search.
        /// </summary>
        /// <value>Primary text to display.</value>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets action to invoke when the footer is selected.
        /// </summary>
        /// <value>Selection action.</value>
        [JsonProperty(PropertyName = "onSelection")]
        public IAction OnSelection { get; set; }
    }
}
