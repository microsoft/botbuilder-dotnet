// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// A clickable action.
    /// </summary>
    public class CardAction
    {
        /// <summary>Initializes a new instance of the <see cref="CardAction"/> class.</summary>
        public CardAction()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="CardAction"/> class.</summary>
        /// <param name="type">
        /// The type of action implemented by this button.
        /// Possible values include: 'openUrl', 'imBack', 'postBack', 'playAudio', 'playVideo', 'showImage', 'downloadFile', 'signin', 'call', 'messageBack', 'openApp'.
        /// </param>
        /// <param name="title">Text description which appears on the button.</param>
        /// <param name="image">Image URL which will appear on the button, next to text label.</param>
        /// <param name="text">Text for this action.</param>
        /// <param name="displayText">(Optional) text to display in the chat feed if the button is clicked.</param>
        /// <param name="value">Supplementary parameter for action. Content of this property depends on the ActionType.</param>
        /// <param name="channelData">Channel-specific data associated with this action.</param>
        public CardAction(string type = default(string), string title = default(string), string image = default(string), string text = default(string), string displayText = default(string), object value = default(object), object channelData = default(object))
        {
            Type = type;
            Title = title;
            Image = image;
            Text = text;
            DisplayText = displayText;
            Value = value;
            ChannelData = channelData;
        }

        /// <summary>
        /// Gets or sets the type of action implemented by this button.
        /// Possible values include: 'openUrl', 'imBack', 'postBack','playAudio', 'playVideo', 'showImage', 'downloadFile', 'signin', 'call', 'messageBack'.
        /// </summary>
        /// <value>The type of action implemented.</value>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        /// <summary>Gets or sets text description which appears on the button.</summary>
        /// <value>The title.</value>
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        /// <summary>Gets or sets image URL which will appear on the button, next to text label.</summary>
        /// <value>The image that appears on the button.</value>
        [JsonProperty(PropertyName = "image")]
        public string Image { get; set; }

        /// <summary>Gets or sets text for this action.</summary>
        /// <value>The text for this action.</value>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        /// <summary>Gets or sets (Optional) text to display in the chat feed if the button is clicked.</summary>
        /// <value>The text to display in the chat feed.</value>
        [JsonProperty(PropertyName = "displayText")]
        public string DisplayText { get; set; }

        /// <summary>Gets or sets supplementary parameter for action. Content of this property depends on the ActionType.</summary>
        /// <value>The supplementary parameter for action.</value>
        [JsonProperty(PropertyName = "value")]
        public object Value { get; set; }

        /// <summary>Gets or sets channel-specific data associated with this action.</summary>
        /// <value>The channel-specific data associated with this action.</value>
        [JsonProperty(PropertyName = "channelData")]
        public object ChannelData { get; set; }

        /// <summary>Gets or sets alternate text to be used for the Image property.</summary>
        /// <value>The alternate text for the image.</value>
        [JsonProperty(PropertyName = "imageAltText")]
        public string ImageAltText { get; set; }

        /// <summary>
        /// Implicit conversion of string to CardAction to simplify creation of
        /// CardActions with string values.
        /// </summary>
        /// <param name="input">input.</param>
        public static implicit operator CardAction(string input) => new CardAction(title: input, value: input);

        /// <summary>
        /// Creates a <see cref="CardAction"/> from the given input.
        /// </summary>
        /// <param name="input">Represents the title and value for the <see cref="CardAction"/>.</param>
        /// <returns>A new <see cref="CardAction"/> instance.</returns>
        public static CardAction FromString(string input)
        {
            return new CardAction(title: input, value: input);
        }
    }
}
