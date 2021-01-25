// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// A thumbnail card (card with a single, small thumbnail image).
    /// </summary>
    public partial class ThumbnailCard
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThumbnailCard"/> class.
        /// </summary>
        public ThumbnailCard()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThumbnailCard"/> class.
        /// </summary>
        /// <param name="title">Title of the card.</param>
        /// <param name="subtitle">Subtitle of the card.</param>
        /// <param name="text">Text for the card.</param>
        /// <param name="images">Array of images for the card.</param>
        /// <param name="buttons">Set of actions applicable to the current
        /// card.</param>
        /// <param name="tap">This action will be activated when user taps on
        /// the card itself.</param>
        public ThumbnailCard(string title = default(string), string subtitle = default(string), string text = default(string), IList<CardImage> images = default(IList<CardImage>), IList<CardAction> buttons = default(IList<CardAction>), CardAction tap = default(CardAction))
        {
            Title = title;
            Subtitle = subtitle;
            Text = text;
            Images = images;
            Buttons = buttons;
            Tap = tap;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets title of the card.
        /// </summary>
        /// <value>The title of the card.</value>
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets subtitle of the card.
        /// </summary>
        /// <value>The subtitle of the card.</value>
        [JsonProperty(PropertyName = "subtitle")]
        public string Subtitle { get; set; }

        /// <summary>
        /// Gets or sets text for the card.
        /// </summary>
        /// <value>The text for the card.</value>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets array of images for the card.
        /// </summary>
        /// <value>The images for the card.</value>
        [JsonProperty(PropertyName = "images")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking compat).
        public IList<CardImage> Images { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets set of actions applicable to the current card.
        /// </summary>
        /// <value>The actions applicable to the current card.</value>
        [JsonProperty(PropertyName = "buttons")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking compat).
        public IList<CardAction> Buttons { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets this action will be activated when user taps on the
        /// card itself.
        /// </summary>
        /// <value>The action that activates when the user taps on the card itself.</value>
        [JsonProperty(PropertyName = "tap")]
        public CardAction Tap { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
