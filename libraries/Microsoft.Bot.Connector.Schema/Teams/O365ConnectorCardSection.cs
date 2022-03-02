// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema.Teams
{
    /// <summary>
    /// O365 connector card section.
    /// </summary>
    public partial class O365ConnectorCardSection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="O365ConnectorCardSection"/> class.
        /// </summary>
        public O365ConnectorCardSection()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="O365ConnectorCardSection"/> class.
        /// </summary>
        /// <param name="title">Title of the section.</param>
        /// <param name="text">Text for the section.</param>
        /// <param name="activityTitle">Activity title.</param>
        /// <param name="activitySubtitle">Activity subtitle.</param>
        /// <param name="activityText">Activity text.</param>
        /// <param name="activityImage">Activity image.</param>
        /// <param name="activityImageType">Describes how Activity image is rendered. Possible values include: 'avatar', 'article'.</param>
        /// <param name="markdown">Use markdown for all text contents. Default value is true.</param>
        /// <param name="facts">Set of facts for the current section.</param>
        /// <param name="images">Set of images for the current section.</param>
        /// <param name="potentialAction">Set of actions for the current section.</param>
        public O365ConnectorCardSection(string title = default, string text = default, string activityTitle = default, string activitySubtitle = default, string activityText = default, string activityImage = default, string activityImageType = default, bool? markdown = default, IList<O365ConnectorCardFact> facts = default, IList<O365ConnectorCardImage> images = default, IList<O365ConnectorCardActionBase> potentialAction = default)
        {
            Title = title;
            Text = text;
            ActivityTitle = activityTitle;
            ActivitySubtitle = activitySubtitle;
            ActivityText = activityText;
            ActivityImage = activityImage;
            ActivityImageType = activityImageType;
            Markdown = markdown;
            Facts = facts;
            Images = images;
            PotentialAction = potentialAction;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets title of the section.
        /// </summary>
        /// <value>The title of the section.</value>
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets text for the section.
        /// </summary>
        /// <value>The text for the section.</value>
        [JsonPropertyName("text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets activity title.
        /// </summary>
        /// <value>The activity title.</value>
        [JsonPropertyName("activityTitle")]
        public string ActivityTitle { get; set; }

        /// <summary>
        /// Gets or sets activity subtitle.
        /// </summary>
        /// <value>The activity subtitle.</value>
        [JsonPropertyName("activitySubtitle")]
        public string ActivitySubtitle { get; set; }

        /// <summary>
        /// Gets or sets activity text.
        /// </summary>
        /// <value>The activity text.</value>
        [JsonPropertyName("activityText")]
        public string ActivityText { get; set; }

        /// <summary>
        /// Gets or sets activity image.
        /// </summary>
        /// <value>The activity image.</value>
        [JsonPropertyName("activityImage")]
        public string ActivityImage { get; set; }

        /// <summary>
        /// Gets or sets describes how Activity image is rendered. Possible
        /// values include: 'avatar', 'article'.
        /// </summary>
        /// <value>The activity image type.</value>
        [JsonPropertyName("activityImageType")]
        public string ActivityImageType { get; set; }

        /// <summary>
        /// Gets or sets use markdown for all text contents. Default value is
        /// true.
        /// </summary>
        /// <value>Boolean indicating whether markdown is used for all text contents.</value>
        [JsonPropertyName("markdown")]
        public bool? Markdown { get; set; }

        /// <summary>
        /// Gets or sets set of facts for the current section.
        /// </summary>
        /// <value>The facts for the current section.</value>
        [JsonPropertyName("facts")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking compat).
        public IList<O365ConnectorCardFact> Facts { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets set of images for the current section.
        /// </summary>
        /// <value>The images for the current section.</value>
        [JsonPropertyName("images")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking compat).
        public IList<O365ConnectorCardImage> Images { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets set of actions for the current section.
        /// </summary>
        /// <value>The actions for the current section.</value>
        [JsonPropertyName("potentialAction")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking compat).
        public IList<O365ConnectorCardActionBase> PotentialAction { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
