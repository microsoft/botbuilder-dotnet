// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Indicates what the payment request is for and the value asked for.
    /// </summary>
    [Obsolete("Bot Framework no longer supports payments.")]
    public partial class PaymentItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentItem"/> class.
        /// </summary>
        public PaymentItem()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentItem"/> class.
        /// </summary>
        /// <param name="label">Human-readable description of the item.</param>
        /// <param name="amount">Monetary amount for the item.</param>
        /// <param name="pending">When set to true this flag means that the
        /// amount field is not final.</param>
        public PaymentItem(string label = default(string), PaymentCurrencyAmount amount = default(PaymentCurrencyAmount), bool? pending = default(bool?))
        {
            Label = label;
            Amount = amount;
            Pending = pending;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets human-readable description of the item.
        /// </summary>
        /// <value>The human-readable label of the item.</value>
        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets monetary amount for the item.
        /// </summary>
        /// <value>The amount for the item.</value>
        [JsonProperty(PropertyName = "amount")]
        public PaymentCurrencyAmount Amount { get; set; }

        /// <summary>
        /// Gets or sets when set to true this flag means that the amount field
        /// is not final.
        /// </summary>
        /// <value>A boolean indicating that amount in field is pending (i.e. not final).</value>
        [JsonProperty(PropertyName = "pending")]
        public bool? Pending { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
