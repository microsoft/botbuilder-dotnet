// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// An item on a receipt card.
    /// </summary>
    public partial class ReceiptItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiptItem"/> class.
        /// </summary>
        public ReceiptItem()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiptItem"/> class.
        /// </summary>
        /// <param name="title">Title of the Card.</param>
        /// <param name="subtitle">Subtitle appears just below Title field,
        /// differs from Title in font styling only.</param>
        /// <param name="text">Text field appears just below subtitle, differs
        /// from Subtitle in font styling only.</param>
        /// <param name="image">Image.</param>
        /// <param name="price">Amount with currency.</param>
        /// <param name="quantity">Number of items of given kind.</param>
        /// <param name="tap">This action will be activated when user taps on
        /// the Item bubble.</param>
        public ReceiptItem(string title = default(string), string subtitle = default(string), string text = default(string), CardImage image = default(CardImage), string price = default(string), string quantity = default(string), CardAction tap = default(CardAction))
        {
            Title = title;
            Subtitle = subtitle;
            Text = text;
            Image = image;
            Price = price;
            Quantity = quantity;
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
        /// Gets or sets subtitle appears just below Title field, differs from
        /// Title in font styling only.
        /// </summary>
        /// <value>The subtitle of the card.</value>
        [JsonProperty(PropertyName = "subtitle")]
        public string Subtitle { get; set; }

        /// <summary>
        /// Gets or sets text field appears just below subtitle, differs from
        /// Subtitle in font styling only.
        /// </summary>
        /// <value>The text field of the card that appears just below the subtitle.</value>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets image.
        /// </summary>
        /// <value>The image.</value>
        [JsonProperty(PropertyName = "image")]
        public CardImage Image { get; set; }

        /// <summary>
        /// Gets or sets amount with currency.
        /// </summary>
        /// <value>The price.</value>
        [JsonProperty(PropertyName = "price")]
        public string Price { get; set; }

        /// <summary>
        /// Gets or sets number of items of given kind.
        /// </summary>
        /// <value>The number of items of a given kind.</value>
        [JsonProperty(PropertyName = "quantity")]
        public string Quantity { get; set; }

        /// <summary>
        /// Gets or sets this action will be activated when user taps on the
        /// Item bubble.
        /// </summary>
        /// <value>The action that will activate when the user taps on the Item bubble.</value>
        [JsonProperty(PropertyName = "tap")]
        public CardAction Tap { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
