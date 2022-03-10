// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// A receipt card.
    /// </summary>
    public partial class ReceiptCard
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiptCard"/> class.
        /// </summary>
        public ReceiptCard()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiptCard"/> class.
        /// </summary>
        /// <param name="title">Title of the card.</param>
        /// <param name="facts">Array of Fact objects.</param>
        /// <param name="items">Array of Receipt Items.</param>
        /// <param name="tap">This action will be activated when user taps on
        /// the card.</param>
        /// <param name="total">Total amount of money paid (or to be
        /// paid).</param>
        /// <param name="tax">Total amount of tax paid (or to be paid).</param>
        /// <param name="vat">Total amount of VAT paid (or to be paid).</param>
        /// <param name="buttons">Set of actions applicable to the current
        /// card.</param>
        public ReceiptCard(string title = default, IList<Fact> facts = default, IList<ReceiptItem> items = default, CardAction tap = default, string total = default, string tax = default, string vat = default, IList<CardAction> buttons = default)
        {
            Title = title;
            Facts = facts ?? new List<Fact>();
            Items = items ?? new List<ReceiptItem>();
            Tap = tap;
            Total = total;
            Tax = tax;
            Vat = vat;
            Buttons = buttons ?? new List<CardAction>();
            CustomInit();
        }

        /// <summary>
        /// Gets or sets title of the card.
        /// </summary>
        /// <value>The title of the card.</value>
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets array of Fact objects.
        /// </summary>
        /// <value>The collection of <see cref="Fact"/>'s.</value>
        [JsonProperty(PropertyName = "facts")]
        public IList<Fact> Facts { get; private set; } = new List<Fact>();

        /// <summary>
        /// Gets array of Receipt Items.
        /// </summary>
        /// <value>The receipt items.</value>
        [JsonProperty(PropertyName = "items")]
        public IList<ReceiptItem> Items { get; private set; } = new List<ReceiptItem>();

        /// <summary>
        /// Gets or sets this action will be activated when user taps on the
        /// card.
        /// </summary>
        /// <value>The card action that will be activated when the user taps on the card.</value>
        [JsonProperty(PropertyName = "tap")]
        public CardAction Tap { get; set; }

        /// <summary>
        /// Gets or sets total amount of money paid (or to be paid).
        /// </summary>
        /// <value>The total amount of money paid (or to be paid).</value>
        [JsonProperty(PropertyName = "total")]
        public string Total { get; set; }

        /// <summary>
        /// Gets or sets total amount of tax paid (or to be paid).
        /// </summary>
        /// <value>The total amount of tax.</value>
        [JsonProperty(PropertyName = "tax")]
        public string Tax { get; set; }

        /// <summary>
        /// Gets or sets total amount of VAT paid (or to be paid).
        /// </summary>
        /// <value>The total amount of VAT.</value>
        [JsonProperty(PropertyName = "vat")]
        public string Vat { get; set; }

        /// <summary>
        /// Gets set of actions applicable to the current card.
        /// </summary>
        /// <value>The actions applicable to the current card.</value>
        [JsonProperty(PropertyName = "buttons")]
        public IList<CardAction> Buttons { get; private set; } = new List<CardAction>();

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
