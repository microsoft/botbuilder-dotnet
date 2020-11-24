// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// O365 connector card section
    /// </summary>
    public partial class O365ConnectorCardSection
    {
        /// <summary>
        /// Initializes a new instance of the O365ConnectorCardSection class.
        /// </summary>
        public O365ConnectorCardSection()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the O365ConnectorCardSection class.
        /// </summary>
        /// <param name="title">Title of the section</param>
        /// <param name="text">Text for the section</param>
        /// <param name="activityTitle">Activity title</param>
        /// <param name="activitySubtitle">Activity subtitle</param>
        /// <param name="activityText">Activity text</param>
        /// <param name="activityImage">Activity image</param>
        /// <param name="activityImageType">Describes how Activity image is
        /// rendered. Possible values include: 'avatar', 'article'</param>
        /// <param name="markdown">Use markdown for all text contents. Default
        /// value is true.</param>
        /// <param name="facts">Set of facts for the current section</param>
        /// <param name="images">Set of images for the current section</param>
        /// <param name="potentialAction">Set of actions for the current
        /// section</param>
        public O365ConnectorCardSection(string title = default(string), string text = default(string), string activityTitle = default(string), string activitySubtitle = default(string), string activityText = default(string), string activityImage = default(string), string activityImageType = default(string), bool? markdown = default(bool?), IList<O365ConnectorCardFact> facts = default(IList<O365ConnectorCardFact>), IList<O365ConnectorCardImage> images = default(IList<O365ConnectorCardImage>), IList<O365ConnectorCardActionBase> potentialAction = default(IList<O365ConnectorCardActionBase>))
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
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets title of the section
        /// </summary>
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets text for the section
        /// </summary>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets activity title
        /// </summary>
        [JsonProperty(PropertyName = "activityTitle")]
        public string ActivityTitle { get; set; }

        /// <summary>
        /// Gets or sets activity subtitle
        /// </summary>
        [JsonProperty(PropertyName = "activitySubtitle")]
        public string ActivitySubtitle { get; set; }

        /// <summary>
        /// Gets or sets activity text
        /// </summary>
        [JsonProperty(PropertyName = "activityText")]
        public string ActivityText { get; set; }

        /// <summary>
        /// Gets or sets activity image
        /// </summary>
        [JsonProperty(PropertyName = "activityImage")]
        public string ActivityImage { get; set; }

        /// <summary>
        /// Gets or sets describes how Activity image is rendered. Possible
        /// values include: 'avatar', 'article'
        /// </summary>
        [JsonProperty(PropertyName = "activityImageType")]
        public string ActivityImageType { get; set; }

        /// <summary>
        /// Gets or sets use markdown for all text contents. Default value is
        /// true.
        /// </summary>
        [JsonProperty(PropertyName = "markdown")]
        public bool? Markdown { get; set; }

        /// <summary>
        /// Gets or sets set of facts for the current section
        /// </summary>
        [JsonProperty(PropertyName = "facts")]
        public IList<O365ConnectorCardFact> Facts { get; set; }

        /// <summary>
        /// Gets or sets set of images for the current section
        /// </summary>
        [JsonProperty(PropertyName = "images")]
        public IList<O365ConnectorCardImage> Images { get; set; }

        /// <summary>
        /// Gets or sets set of actions for the current section
        /// </summary>
        [JsonProperty(PropertyName = "potentialAction")]
        public IList<O365ConnectorCardActionBase> PotentialAction { get; set; }

    }
}
