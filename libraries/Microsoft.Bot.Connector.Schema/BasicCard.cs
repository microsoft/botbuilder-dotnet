// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary> A basic card.</summary>
    public class BasicCard
    {
        /// <summary>Initializes a new instance of the <see cref="BasicCard"/> class.</summary>
        public BasicCard()
        {
            CustomInit();
        }

        /// <summary>Initializes a new instance of the <see cref="BasicCard"/> class.</summary>
        /// <param name="title">Title of the card.</param>
        /// <param name="subtitle">Subtitle of the card.</param>
        /// <param name="text">Text for the card.</param>
        /// <param name="images">Array of images for the card.</param>
        /// <param name="buttons">Set of actions applicable to the current card.</param>
        /// <param name="tap">This action will be activated when user taps on the card itself.</param>
        public BasicCard(string title = default, string subtitle = default, string text = default, IList<CardImage> images = default, IList<CardAction> buttons = default, CardAction tap = default)
        {
            Title = title;
            Subtitle = subtitle;
            Text = text;
            Images = images;
            Buttons = buttons;
            Tap = tap;
            CustomInit();
        }

        /// <summary>Gets or sets title of the card.</summary>
        /// <value>The title of the card.</value>
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>Gets or sets subtitle of the card.</summary>
        /// <value>The subtitle of the card.</value>
        [JsonPropertyName("subtitle")]
        public string Subtitle { get; set; }

        /// <summary>Gets or sets text for the card.</summary>
        /// <value>The text of the card.</value>
        [JsonPropertyName("text")]
        public string Text { get; set; }

        /// <summary>Gets or sets list of images for the card.</summary>
        /// <value>A list of images for the card.</value>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Property setter is required for the collection to be deserialized")]
        [JsonPropertyName("images")]
        public IList<CardImage> Images { get; set; }

        /// <summary>Gets or sets set of actions applicable to the current card.</summary>
        /// <value>The actions applicable to the current card.</value>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Property setter is required for the collection to be deserialized")]
        [JsonPropertyName("buttons")]
        public IList<CardAction> Buttons { get; set; }

        /// <summary>Gets or sets this action will be activated when user taps on the card itself.</summary>
        /// <value>The action that will activate when user taps card.</value>
        [JsonPropertyName("tap")]
        public CardAction Tap { get; set; }

        /// <summary>An initialization method that performs custom operations like setting defaults.</summary>
        private void CustomInit()
        {
        }
    }
}
