// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// O365 connector card.
    /// </summary>
    public partial class O365ConnectorCard
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="O365ConnectorCard"/> class.
        /// </summary>
        public O365ConnectorCard()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="O365ConnectorCard"/> class.
        /// </summary>
        /// <param name="title">Title of the item.</param>
        /// <param name="text">Text for the card.</param>
        /// <param name="summary">Summary for the card.</param>
        /// <param name="themeColor">Theme color for the card.</param>
        /// <param name="sections">Set of sections for the current card.</param>
        /// <param name="potentialAction">Set of actions for the current
        /// card.</param>
        public O365ConnectorCard(string title = default, string text = default, string summary = default, string themeColor = default, IList<O365ConnectorCardSection> sections = default, IList<O365ConnectorCardActionBase> potentialAction = default)
        {
            Title = title;
            Text = text;
            Summary = summary;
            ThemeColor = themeColor;
            Sections = sections;
            PotentialAction = potentialAction;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets title of the item.
        /// </summary>
        /// <value>The title of the item.</value>
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets text for the card.
        /// </summary>
        /// <value>The text for the card.</value>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets summary for the card.
        /// </summary>
        /// <value>The summary for the card.</value>
        [JsonProperty(PropertyName = "summary")]
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets theme color for the card.
        /// </summary>
        /// <value>The theme color for the card.</value>
        [JsonProperty(PropertyName = "themeColor")]
        public string ThemeColor { get; set; }

        /// <summary>
        /// Gets or sets set of sections for the current card.
        /// </summary>
        /// <value>The sections for the current card.</value>
        [JsonProperty(PropertyName = "sections")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking compat).
        public IList<O365ConnectorCardSection> Sections { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets set of actions for the current card.
        /// </summary>
        /// <value>The actions for the current card.</value>
        [JsonProperty(PropertyName = "potentialAction")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking compat).
        public IList<O365ConnectorCardActionBase> PotentialAction { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
